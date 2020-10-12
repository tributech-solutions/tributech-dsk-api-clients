using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.Collections.Generic;
using System.Linq;
using Tributech.Dsk.Api.Clients;
using Tributech.Dsk.Api.Clients.DataApi;

namespace Tributech.Dataspace.ClientExamples {
    class Program {
        // Api Client Config
        private const string nodeUrl = "http://data-api.your-node.dataspace-node.com";
        private const string tokenUrl = "https://id.your-hub.dataspace-hub.com/connect/token";
        private const string scope = "data-api-endpoint";
        private const string clientId = "<your-node-specific-api-client>";
        private const string clientSecret = "<your-node-specific-api-client-secret>";

        private static readonly Guid dataStreamId = new Guid("0514974b-fe05-4264-8c63-a636c4bea6a0");

        static async Task Main(string[] args) {
            var authHandler = new APIAuthHandler(tokenUrl, scope, clientId, clientSecret);
            using (var authorizedHttpClient = new HttpClient(authHandler)) {
                var apiClient = new DataAPIClient(nodeUrl, authorizedHttpClient);

                // Get data points within the last 7 days
                ICollection<ReadValueDoubleModel> data = await apiClient.GetValuesAsDoubleAsync(dataStreamId, DateTime.Now, DateTime.Now.AddDays(-7), "asc", pageNumber: null, pageSize: null);

                foreach(var item in data) {
                    Console.WriteLine($"{item.Timestamp}: Value {item.Values.FirstOrDefault()}");
                }
            }
        }
    }
}
