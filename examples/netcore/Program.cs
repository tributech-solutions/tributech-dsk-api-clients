using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.Collections.Generic;
using System.Linq;
using Tributech.Dataspace.Clients;
using Tributech.DataSpace.Clients.NetCore;

namespace Tributech.Dataspace.ClientExamples {
    class Program {
        // Api Client Config
        private const string nodeUrl = "https://node.example.com";
        private const string tokenUrl = "https://id.example.com";
        private const string scope = "195d0b59-2113-4d67-ac6c-7116595e413b-full-access";
        private const string clientId = "4c90a8bf-10de-4fe5-ae44-fcd4e9bdb602";
        private const string clientSecret = "56af011e-7c22-48e7-a4be-fb867b0be504";

        private static readonly Guid dataStreamId = new Guid("0514974b-fe05-4264-8c63-a636c4bea6a0");
        
        static async Task Main(string[] args) {
            var authHandler = new APIAuthHandler(tokenUrl, scope, clientId, clientSecret);
            using (var authorizedHttpClient = new HttpClient(authHandler)) {
                var apiClient = new DataAPIClient(nodeUrl, authorizedHttpClient);

                // Get data points within the last 7 days
                ICollection<ReadValueDoubleModel> data = await apiClient.GetValuesAsDoubleAsync(dataStreamId, DateTime.Now, DateTime.Now.AddDays(-7), fromSyncNumber: null, "asc", pageNumber: null, pageSize: null);

                foreach(var item in data) {
                    Console.WriteLine($"{item.Timestamp}: Value {item.Values.FirstOrDefault()}");
                }
            }
        }
    }
}
