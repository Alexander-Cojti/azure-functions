using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.Storage.Blob;
using System;

namespace blobtrigger
{
    public static class Function1
    {
        public class Book
        {
            public string Id { get; set; }
            public string FileName { get; set; }
        }
        [FunctionName("Function1")]
        public static async Task RunAsync(
    [BlobTrigger("datum/{name}", Connection = "Search")] CloudBlockBlob myBlob,
    [CosmosDB("taskDatabase", "TaskCollection", ConnectionStringSetting = "CosmosDB")] IAsyncCollector<object> todos, string name, ILogger log)
        {
            SearchServiceClient serviceClient = new SearchServiceClient(searchServiceName: "jfk-search-service-qymjpzort5hho", credentials: new SearchCredentials("023DB82430FFA416AD39EEF8A6FDFF2A"));
            ISearchIndexClient indexClient = serviceClient.Indexes.GetClient("jfkindex");
            await serviceClient.Indexers.RunAsync("jfkindexer");
            try
            {
                IndexerExecutionInfo execInfo = serviceClient.Indexers.GetStatus("jfkindexer");
                Console.WriteLine("{0} veces ejecutadas.", execInfo.ExecutionHistory.Count);
                Console.WriteLine("Indexer Status: " + execInfo.Status.ToString());

                IndexerExecutionResult result = execInfo.LastResult;

                Console.WriteLine("Latest run");
                Console.WriteLine("  Run Status: {0}", result.Status.ToString());
                Console.WriteLine("  Total Documents: {0}, Failed: {1}", result.ItemCount, result.FailedItemCount);

                TimeSpan elapsed = result.EndTime.Value - result.StartTime.Value;
                Console.WriteLine("  StartTime: {0:T}, EndTime: {1:T}, Elapsed: {2:t}", result.StartTime.Value, result.EndTime.Value, elapsed);

                string errorMsg = (result.ErrorMessage == null) ? "none" : result.ErrorMessage;
                Console.WriteLine("  ErrorMessage: {0}", errorMsg);
                Console.WriteLine("  Document Errors: {0}, Warnings: {1}\n", result.Errors.Count, result.Warnings.Count);
            }
            catch (Exception e) { Console.WriteLine("Error: ", e); }
            var param = new SearchParameters
            {
                Select = new[] { "*" },
                //IncludeTotalResultCount = true
            };
            System.Threading.Thread.Sleep(19000);
            log.LogInformation($"espera terminada{name}");
            var results = indexClient.Documents.Search<Book>($"{name}", param);
            log.LogInformation($"enviando a cosmos.... {myBlob.Name}");
            await todos.AddAsync(results);
        }
    }
}







