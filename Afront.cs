using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Auth;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace BloApi
{
    public static class blobOperations
    {
        [FunctionName("ListBlob")]
        public static async Task<List<string>> ListBlob(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "List")] HttpRequest req)
        {
            StorageCredentials storageCredentials = new StorageCredentials("jfkstorageqymjpzort5hho",
                 "CamEKgqVaylmQta0aUTqN8vUy/xvUewVYZOrBvF1mh85zlgtj3fm7YheYRgoB6paMp+VcFUpTRIlow2VHlyCww==");
            CloudStorageAccount storageAccount = new CloudStorageAccount(storageCredentials, useHttps: true);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("datum");
            List<string> blobs = new List<string>();
            BlobResultSegment resultSegment = await container.ListBlobsSegmentedAsync(null);
            foreach (IListBlobItem item in resultSegment.Results)
            {
                //Console.WriteLine("- {0} (type: {1})", item.Uri, item.GetType());
                if (item.GetType() == typeof(CloudBlockBlob))
                {
                    CloudBlockBlob blob = (CloudBlockBlob)item;
                    blobs.Add(blob.Name);
                }
            }
            return blobs;
        }

        [FunctionName("DeleteBlob")]
        public static async Task<IActionResult> DeleteBlob(
           [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "Delete/{name}")] HttpRequest req, string name)
        {
            StorageCredentials storageCredentials = new StorageCredentials("jfkstorageqymjpzort5hho",
                "CamEKgqVaylmQta0aUTqN8vUy/xvUewVYZOrBvF1mh85zlgtj3fm7YheYRgoB6paMp+VcFUpTRIlow2VHlyCww==");
            CloudStorageAccount storageAccount = new CloudStorageAccount(storageCredentials, true);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("datum");
            var blobName = name;
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(blobName);
            await blockBlob.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots, null, null, null);

            return (ActionResult)new OkObjectResult($"blob {blobName} deleted successful");
        }

        [FunctionName("DownloadFile")]
        public static async Task<IActionResult> Download(
           [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Download/{name}")] HttpRequest req, string name)
        {
            StorageCredentials storageCredentials = new StorageCredentials("jfkstorageqymjpzort5hho",
                "CamEKgqVaylmQta0aUTqN8vUy/xvUewVYZOrBvF1mh85zlgtj3fm7YheYRgoB6paMp+VcFUpTRIlow2VHlyCww==");
            CloudStorageAccount storageAccount = new CloudStorageAccount(storageCredentials, useHttps: true);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("datum");
            var blobName = name;
            var newBlob = container.GetBlockBlobReference(blobName);
            await newBlob.DownloadToFileAsync($"C:/users/PC/Desktop/copia_{blobName}", FileMode.Create);

            return (ActionResult)new OkObjectResult($"download soccessful, into C:/ users / PC / Desktop / copia_{ blobName }");
        }

        [FunctionName("DeleteAll")]
        public static async Task<ActionResult> DeleteAll(
           [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "DeleteAll")] HttpRequest req)
        {
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
                await blob.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots, null, null, null);

            }
            return (ActionResult)new OkObjectResult("deleted containter content successful");
        }

    }
}
