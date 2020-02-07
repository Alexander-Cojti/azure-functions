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

namespace ResetAll
{
    public static class FunctionReset
    {

        [FunctionName("ResetAll")]
        public static async System.Threading.Tasks.Task<ActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "ResetAll")] HttpRequest req,
        [CosmosDB(ConnectionStringSetting = "CosmosDB")]DocumentClient client)
        {
            //Credentials
            SearchIndexClient indexClient = new SearchIndexClient("jfk-search-service-qymjpzort5hho", "jfkindex", new SearchCredentials("023DB82430FFA416AD39EEF8A6FDFF2A"));
            StorageCredentials storageCredentials = new StorageCredentials("jfkstorageqymjpzort5hho",
            "CamEKgqVaylmQta0aUTqN8vUy/xvUewVYZOrBvF1mh85zlgtj3fm7YheYRgoB6paMp+VcFUpTRIlow2VHlyCww==");
            CloudStorageAccount storageAccount = new CloudStorageAccount(storageCredentials, useHttps: true);
            CloudBlobContainer container = storageAccount.CreateCloudBlobClient().GetContainerReference("datum");
            //wait to execute delete "Index-batch"    
            while (true)
            {
                DocumentSearchResult<Microsoft.Azure.Search.Models.Document> searchResult = indexClient.Documents.Search<Microsoft.Azure.Search.Models.Document>(string.Empty);
                List<string> azureDocsToDelete =
                searchResult.Results
                    .Select(r => r.Document["id"].ToString()).ToList();
                if (azureDocsToDelete.Count != 0)
                {
                    try
                    {
                        var batch = IndexBatch.Delete("id", azureDocsToDelete);
                        var result = indexClient.Documents.Index(batch);
                    }
                    catch (IndexBatchException ex)
                    {
                        throw new Exception($"Failed to reset the index: {string.Join(", ", ex.IndexingResults.Where(r => !r.Succeeded).Select(r => r.Key))}");
                    }
                }
                else
                {
                    break;
                }
            }
            //delete all blockblobs "datum-container"
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
            //create an instance "TaskDatabase-collection"
            Database db = client.CreateDatabaseQuery().Where(d => d.Id == "taskDatabase").AsEnumerable().SingleOrDefault();
            DocumentCollection coll = client.CreateDocumentCollectionQuery(db.SelfLink).Where(c => c.Id == "TaskCollection").ToList().SingleOrDefault();
            var docs = client.CreateDocumentQuery(coll.DocumentsLink);
            foreach (var doc in docs)
            {
                await client.DeleteDocumentAsync(doc.SelfLink);
            }
            Console.WriteLine("11111");
            //delete collection "Recorrido-container"
            DocumentCollection collection = client.CreateDocumentCollectionQuery(db.SelfLink).Where(a => a.Id == "recorrido").ToList().SingleOrDefault();
            var docs2 = client.CreateDocumentQuery(collection.DocumentsLink);
            foreach (var doc2 in docs2)
            {
                await client.DeleteDocumentAsync(doc2.SelfLink, new RequestOptions { PartitionKey = new PartitionKey(doc2.Id) });
            }
            Console.WriteLine("2222");
            return (ActionResult)new OkObjectResult($" success executed function reset All ");
        }
    }
}
