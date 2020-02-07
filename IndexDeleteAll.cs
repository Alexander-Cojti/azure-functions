using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace blobtrigger
{
    public static class Function1
    {
        [FunctionName("DeleteAll")]
        public static ActionResult RunAsync(
   [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "indexDeleteAll")] HttpRequest req,
            ILogger log)
        {
            SearchServiceClient serviceClient = new SearchServiceClient(searchServiceName: "jfk-search-service-qymjpzort5hho", credentials: new SearchCredentials("023DB82430FFA416AD39EEF8A6FDFF2A"));
            ISearchIndexClient indexClient = serviceClient.Indexes.GetClient("jfkindex");
            DocumentSearchResult<Document> searchResult;
            try
            {
                searchResult = indexClient.Documents.Search<Document>(string.Empty);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al recorrer index");
            }
            List<string> azureDocsToDelete =
                searchResult
                    .Results
                    .Select(r => r.Document["id"].ToString())
                    .ToList();
            try
            {
                var batch = IndexBatch.Delete("id", azureDocsToDelete);
                var result = indexClient.Documents.Index(batch);
            }
            catch (IndexBatchException ex)
            {
                throw new Exception($"error al eliminar documentos : {string.Join(", ", ex.IndexingResults.Where(r => !r.Succeeded).Select(r => r.Key))}");
            }
            return (ActionResult)new OkObjectResult($" function ejecutado");
        }
    }
}






