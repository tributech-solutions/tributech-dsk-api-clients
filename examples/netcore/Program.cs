using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.Collections.Generic;
using System.Linq;
using Tributech.Dsk.Api.Clients;
using Tributech.Dsk.Api.Clients.DataApi;

namespace Tributech.Dataspace.ClientExamples {
    class Program {
        // Api Client Configs:

        // Your Node URL (replace "your-node" with the name of your node)
        private const string nodeUrl = "https://data-api.your-node.dataspace-node.com";
        // Your Hub URL (replace "your-hub" with the name of your hub and "your-node" with the name of your node)
        private const string tokenUrl = "https://auth.your-hub.dataspace-hub.com/auth/realms/your-node/protocol/openid-connect/token";
        // the scope setting defines what parts of an api / endpoints should be accessible
        // in this case it is either data-api-endpoint for the Data API or data-api-endpoint trust-api-endpoint for the Trust API.
        // The Trust API requires both scopes since it comes with the DSK Agent Integrated which passes through values to the Data API
        private const string scope = "profile email data-api node-id";
        // The following two settings can be found in the DataSpace Admin App (Profile -> Administration)
        private const string clientId = "<your-api-specific-client-id>";
        private const string clientSecret = "<your-api-specific-api-client-secret>";

        private static readonly Guid dataStreamId = new Guid("0514974b-fe05-4264-8c63-a636c4bea6a0");

        static async Task Main(string[] args) {
            var authHandler = new APIAuthHandler(tokenUrl, scope, clientId, clientSecret);
            using (var authorizedHttpClient = new HttpClient(authHandler)) {
                var apiClient = new DataAPIClient(nodeUrl, authorizedHttpClient);

                // Get data points within the last 7 days
                ICollection<ReadValueDoubleModel> data = await apiClient.GetValuesAsDoubleAsync(dataStreamId, DateTime.Now.AddDays(-7), DateTime.Now, "asc", pageNumber: null, pageSize: null);

                foreach(var item in data) {
                    Console.WriteLine($"{item.Timestamp}: Value {item.Values.FirstOrDefault()}");
                }
            }
        }
    }
}
