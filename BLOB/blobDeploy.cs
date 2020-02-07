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
using System.Net.Http.Headers;
using System.Net.Http;
using System.Net;
using System.Drawing;

namespace BloApi
{
    public static class BlobOperations
    {
        [FunctionName("GetAllBlobs")]
        public static async Task<List<string>> ListBlob(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "GetAllBlob")] HttpRequest req)
        {
            StorageCredentials storageCredentials = new StorageCredentials("jfkstorageqymjpzort5hho",
                 "CamEKgqVaylmQta0aUTqN8vUy/xvUewVYZOrBvF1mh85zlgtj3fm7YheYRgoB6paMp+VcFUpTRIlow2VHlyCww==");
            CloudStorageAccount storageAccount = new CloudStorageAccount(storageCredentials, true);
            CloudBlobContainer container = storageAccount.CreateCloudBlobClient().GetContainerReference("datum");
            List<string> blobs = new List<string>();
            BlobResultSegment resultSegment = await container.ListBlobsSegmentedAsync(null);
            foreach (IListBlobItem item in resultSegment.Results)
            {
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
           [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "DeleteBlob/{name}")] HttpRequest req, string name)
        {
            StorageCredentials storageCredentials = new StorageCredentials("jfkstorageqymjpzort5hho",
                "CamEKgqVaylmQta0aUTqN8vUy/xvUewVYZOrBvF1mh85zlgtj3fm7YheYRgoB6paMp+VcFUpTRIlow2VHlyCww==");
            CloudStorageAccount storageAccount = new CloudStorageAccount(storageCredentials, true);
            CloudBlobContainer container = storageAccount.CreateCloudBlobClient().GetContainerReference("datum");
            var blobName = name;
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(blobName);
            await blockBlob.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots, null, null, null);
            return (ActionResult)new OkObjectResult($"blob {blobName} deleted successful");
        }
        [FunctionName("DownloadBlobs")]
        public static async Task<HttpResponseMessage> DownloadBlob(
          [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "DownloadBlob/{name}")] HttpRequest req, string name)
        {
            StorageCredentials storageCredentials = new StorageCredentials("jfkstorageqymjpzort5hho",
                "CamEKgqVaylmQta0aUTqN8vUy/xvUewVYZOrBvF1mh85zlgtj3fm7YheYRgoB6paMp+VcFUpTRIlow2VHlyCww==");
            CloudStorageAccount storageAccount = new CloudStorageAccount(storageCredentials, true);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("datum");
            var blobName = name;
            CloudBlockBlob block = container.GetBlockBlobReference(blobName);
            HttpResponseMessage message = new HttpResponseMessage(HttpStatusCode.OK);
            Stream blobStream = await block.OpenReadAsync();
            message.Content = new StreamContent(blobStream);
            message.Content.Headers.ContentLength = block.Properties.Length;
            message.StatusCode = HttpStatusCode.OK;
            message.Content.Headers.ContentType = new MediaTypeHeaderValue(block.Properties.ContentType);
            message.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = $"CopyOf_{block.Name}",
                Size = block.Properties.Length
            };
            return message;
        }
    }
}

