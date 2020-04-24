using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Tributech.DataSpace.Clients.NetCore {
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

        private readonly object _lockobj = new object();

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
                lock(_lockobj) {
                    if (DateTime.Now.Ticks > _tokenValidUntil.Ticks) {
                        RefreshToken(cancellationToken).Wait(); // cannot await inside of lock
                    }
                }
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
