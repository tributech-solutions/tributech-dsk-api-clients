using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Tributech.Dataspace.Clients {
    public class APIAuthHandler: DelegatingHandler {
        // refresh the token x sec before it actually expires
        private const int tokenExpiryGracePeriod = 5;

        private class AuthResponse {
            public string access_token;
            public int expires_in;
        }

        private string authUrl;

        private string token = null;

        private DateTime tokenValidUntil = DateTime.MinValue;

        private readonly string scope;

        private readonly string clientId;

        private readonly string clientSecret;

        private readonly object _lockobj = new object(); 

        public APIAuthHandler(string authUrl, string scope, string clientId, string clientSecret)
        :this(authUrl, scope, clientId, clientSecret, new HttpClientHandler()) { } 

        public APIAuthHandler(string authUrl, string scope, string clientId, string clientSecret, HttpMessageHandler handler) 
        :base(handler) {
            this.authUrl = authUrl;
            this.scope = scope;
            this.clientId = clientId;
            this.clientSecret = clientSecret;
        }

        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
            if (DateTime.Now.Ticks > tokenValidUntil.Ticks) {
                lock(_lockobj) {
                    if (DateTime.Now.Ticks > tokenValidUntil.Ticks) {
                        RefreshToken(cancellationToken).Wait(); // cannot await inside of lock
                    }
                }
            }

            request.Headers.Add("Authorization", $"Bearer {this.token}");
            return await base.SendAsync(request, cancellationToken);
        }

        private async Task RefreshToken(CancellationToken cancellationToken) {
            var now = DateTime.Now;
            var values = new Dictionary<string, string>()
            {
                ["grant_type"] = "client_credentials",
                ["scope"] = scope
            };
            var postBody = new FormUrlEncodedContent(values);
            var request = new HttpRequestMessage(HttpMethod.Post, authUrl);
            request.Content = postBody;
            var byteArray = new UTF8Encoding().GetBytes($"{clientId}:{clientSecret}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
            var response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode != HttpStatusCode.OK) {
                throw new Exception("Tributech API Authorization failed");
            }

            var responseBodyString = await response.Content.ReadAsStringAsync();
            var responseBody = JsonConvert.DeserializeObject<AuthResponse>(responseBodyString);

            this.token = responseBody.access_token;
            int expiresIn = responseBody.expires_in;
            this.tokenValidUntil = now.AddSeconds(expiresIn - tokenExpiryGracePeriod);
        }
    }
}