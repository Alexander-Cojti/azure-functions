using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using System.Linq;

namespace get
{
    public static class pagination
    {
        [FunctionName("get")]
        public static async Task<IActionResult> GetRecords(
       [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
       [CosmosDB(databaseName: "taskDatabase", collectionName: "Boleta", ConnectionStringSetting = "Connection")]DocumentClient client)
        {
            var queryParams = req.GetQueryParameterDictionary();
            //max records to return (default to 50)
            var count = Int32.Parse(queryParams.FirstOrDefault(q => q.Key == "count").Value ?? "5");
            // continuation token(for paging)
            var continuationToken = queryParams.FirstOrDefault(q => q.Key == "continuationToken").Value;
            //Buil query options
            var feedOptions = new FeedOptions()
            {
                MaxItemCount = count,
                RequestContinuation = continuationToken,
                  EnableCrossPartitionQuery = true
            };
            var uri = UriFactory.CreateDocumentCollectionUri("taskDatabase", "Boleta");
            var query = client.CreateDocumentQuery(uri, feedOptions)
                .AsDocumentQuery();
            var results = await query.ExecuteNextAsync();
            return new OkObjectResult(new
            {
                hasMoreResults = query.HasMoreResults,
                pagingToken = query.HasMoreResults ? results.ResponseContinuation : null,
                results = results.ToList()
            });
        }
    }
}