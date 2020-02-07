using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Documents.Client;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Azure.Documents;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Auth;

namespace DeleteAll
{
    public class DeleteAll
    {
        [FunctionName("DeleteAll")]
        public static async Task<ActionResult> DeleteAllCosmosStorage(
       [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "DeleteAllDocBlobs")] HttpRequest req,
       [CosmosDB(ConnectionStringSetting = "Connection")]DocumentClient client)
        {
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
            return (ActionResult)new OkObjectResult("deleted all blobs Storage and Items Cosmos successful");
        }
    }
}