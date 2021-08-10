using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Tributech.Dsk.Api.Clients {
	public class APIAuthHandler: DelegatingHandler {
		// refresh the token x sec before it actually expires
		private const int TokenExpiryGracePeriod = 5;

		private class AuthResponse {
			public string Access_token { get; set; }
			public int Expires_in { get; set; }
		}

		private readonly string _authUrl;

		private string _token;

		private DateTime _tokenValidUntil = DateTime.MinValue;

		private readonly string _scope;

		private readonly string _clientId;

		private readonly string _clientSecret;

		private readonly SemaphoreSlim _refreshTokenLock = new SemaphoreSlim(1, 1);

		/// <summary>
		/// Creates an instance of the APIAuthHandler for access to the DSK APIs.
		/// </summary>
		/// <param name="authUrl">Your Hub URL (e.g. https://id.your-hub.dataspace-hub.com/connect/token, replace your-hub with the name of your hub)</param>
		/// <param name="scope">the scope setting defines what parts of an api / endpoints should be accessible
		/// in this case it is either data-api-endpoint for the Data API or data-api-endpoint trust-api-endpoint for the Trust API.
		/// The Trust API requires both scopes since it comes with the DSK Agent Integrated which passes through values to the Data API.
		/// </param>
		/// <param name="clientId">Your client id for authentication -> can be found in the DataSpace Admin App (Profile -> Administration)</param>
		/// <param name="clientSecret">Your client id for authentication -> can be found in the DataSpace Admin App (Profile -> Administration)</param>
		public APIAuthHandler(string authUrl, string scope, string clientId, string clientSecret)
		:this(authUrl, scope, clientId, clientSecret, new HttpClientHandler()) { }

		public APIAuthHandler(string authUrl, string scope, string clientId, string clientSecret, HttpMessageHandler handler)
		:base(handler) {
			_authUrl = authUrl;
			_scope = scope;
			_clientId = clientId;
			_clientSecret = clientSecret;
		}

		protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {

			if (DateTime.Now.Ticks > _tokenValidUntil.Ticks) {

				await _refreshTokenLock.WaitAsync();
				try {
					if (DateTime.Now.Ticks > _tokenValidUntil.Ticks) {
						await RefreshToken(cancellationToken);
					}
				}
				finally { _refreshTokenLock.Release(); }
			}

			request.Headers.Add("Authorization", $"Bearer {_token}");
			return await base.SendAsync(request, cancellationToken);
		}

		private async Task RefreshToken(CancellationToken cancellationToken) {
			var now = DateTime.Now;
			var values = new Dictionary<string, string>()
			{
				["grant_type"] = "client_credentials",
				["scope"] = _scope
			};
			var postBody = new FormUrlEncodedContent(values);
			var request = new HttpRequestMessage(HttpMethod.Post, _authUrl);
			request.Content = postBody;
			var byteArray = new UTF8Encoding().GetBytes($"{_clientId}:{_clientSecret}");
			request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
			var response = await base.SendAsync(request, cancellationToken);

			if (response.StatusCode != HttpStatusCode.OK) {
				throw new Exception("Tributech API Authorization failed");
			}

			var responseBodyString = await response.Content.ReadAsStringAsync();
			var responseBody = JsonConvert.DeserializeObject<AuthResponse>(responseBodyString);

			_token = responseBody.Access_token;
			int expiresIn = responseBody.Expires_in;
			_tokenValidUntil = now.AddSeconds(expiresIn - TokenExpiryGracePeriod);
		}
	}
}
