using Microsoft.Azure.WebJobs;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace blobtrigger
{
    public static class Function1
    {
        [FunctionName("DeleteAll")]
        public static async System.Threading.Tasks.Task<ActionResult> RunAsync(
[HttpTrigger(AuthorizationLevel.Function, "delete", Route = "indexDeleteAll")] HttpRequest req,
[CosmosDB(ConnectionStringSetting = "CosmosDB")]DocumentClient client, ILogger log)
        {
            SearchServiceClient serviceClient = new SearchServiceClient(searchServiceName: "jfk-search-service-qymjpzort5hho", credentials: new SearchCredentials("023DB82430FFA416AD39EEF8A6FDFF2A"));
            ISearchIndexClient indexClient = serviceClient.Indexes.GetClient("jfkindex");
            DocumentSearchResult<Microsoft.Azure.Search.Models.Document> searchResult;
            try
            {
                searchResult = indexClient.Documents.Search<Microsoft.Azure.Search.Models.Document>(string.Empty);
            }
            catch (Exception e)
            {
                throw new Exception("Error al recorrer index", e);
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
            Database db = client.CreateDatabaseQuery()
                               .Where(d => d.Id == "taskDatabase")
                               .AsEnumerable()
                               .SingleOrDefault();
            DocumentCollection coll = client.CreateDocumentCollectionQuery(db.SelfLink)
                                            .Where(c => c.Id == "TaskCollection")
                                            .ToList()
                                            .SingleOrDefault();
            var docs = client.CreateDocumentQuery(coll.DocumentsLink);
            foreach (var doc in docs)
            {
                await client.DeleteDocumentAsync(doc.SelfLink);
            }  
            DocumentCollection coll2 = client.CreateDocumentCollectionQuery(db.SelfLink)
                                            .Where(c => c.Id == "recorrido")
                                            .ToList()
                                            .SingleOrDefault();
            var docs2 = client.CreateDocumentQuery(coll.DocumentsLink);
            foreach (var doc in docs)
            {
                await client.DeleteDocumentAsync(doc.SelfLink);
            }
            StorageCredentials storageCredentials = new StorageCredentials("jfkstorageqymjpzort5hho",
                "CamEKgqVaylmQta0aUTqN8vUy/xvUewVYZOrBvF1mh85zlgtj3fm7YheYRgoB6paMp+VcFUpTRIlow2VHlyCww==");
            CloudStorageAccount storageAccount = new CloudStorageAccount(storageCredentials, useHttps: true);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("datum");
            List<string> blobs = new List<string>();
            BlobResultSegment resultSegment = await container.ListBlobsSegmentedAsync(null);
            foreach (IListBlobItem item in resultSegment.Results)
            {
                CloudBlockBlob blob = (CloudBlockBlob)item;
                blobs.Add(blob.Name);
            }
            foreach (var file in blobs)
            {
                CloudBlockBlob blob = container.GetBlockBlobReference(file);
                await blob.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots, null, null, null);
            }
            return (ActionResult)new OkObjectResult($" function ejecutado");
        }
    }
}






