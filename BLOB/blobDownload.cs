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
using System;
using System.Net.Http;
using System.Net;
using System.Net.Http.Headers;

namespace BlobStorageDQ
{
    public static class BlobStorageDQ
    {
        [FunctionName("DownloadFile")]
        public static async Task<IActionResult> Download(
                  [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "download/{name}")] HttpRequest req, string name)
        {
            StorageCredentials storageCredentials = new StorageCredentials("jfkstorageqymjpzort5hho", "CamEKgqVaylmQta0aUTqN8vUy/xvUewVYZOrBvF1mh85zlgtj3fm7YheYRgoB6paMp+VcFUpTRIlow2VHlyCww==");
            CloudStorageAccount storageAccount = new CloudStorageAccount(storageCredentials, useHttps: true);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("datum");
            var blobName = name;
            var newBlob = container.GetBlockBlobReference(blobName);
            await newBlob.DownloadToFileAsync($"C:/users/PC/Desktop/copia_{blobName}", FileMode.Create);
            return (ActionResult)new OkObjectResult($"download soccesful, into C:/ users / PC / Desktop / copia_{ blobName }");
        }

        [FunctionName("DownloadBlob")]
        public static async Task<IActionResult> DownloadBlob(
         [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "DownloadBlob/{name}")] HttpRequest req, string name)
        {
            StorageCredentials storageCredentials = new StorageCredentials("jfkstorageqymjpzort5hho",
                "CamEKgqVaylmQta0aUTqN8vUy/xvUewVYZOrBvF1mh85zlgtj3fm7YheYRgoB6paMp+VcFUpTRIlow2VHlyCww==");
            CloudStorageAccount storageAccount = new CloudStorageAccount(storageCredentials, useHttps: true);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("datum");
            var blobName = name;
            var newBlob = container.GetBlockBlobReference(blobName);

            using (var fileStream = System.IO.File.OpenWrite($@"./copia_{blobName}"))
            {
                await newBlob.DownloadToStreamAsync(fileStream);
            }
            return new OkObjectResult($"download soccessful, into @ ./ copia_{blobName}");
        }
    }
}

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
using System;
using System.Net.Http;
using System.Net;
using System.Net.Http.Headers;

namespace BlobStorageDQ
{
    public static class BlobStorageDQ
    {
        [FunctionName("DownloadFile")]
        public static async Task<IActionResult> Download(
                  [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "DownloadBlob/{name}")] HttpRequest req, string name)
        {
            StorageCredentials storageCredentials = new StorageCredentials("jfkstorageqymjpzort5hho", "CamEKgqVaylmQta0aUTqN8vUy/xvUewVYZOrBvF1mh85zlgtj3fm7YheYRgoB6paMp+VcFUpTRIlow2VHlyCww==");
            CloudStorageAccount storageAccount = new CloudStorageAccount(storageCredentials, useHttps: true);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("datum");
            var blobName = name;
            var newBlob = container.GetBlockBlobReference(blobName);
            await newBlob.DownloadToFileAsync($@"C:/users/PC/Desktop/copia_{blobName}", FileMode.Create);
            return (ActionResult)new OkObjectResult($"download soccesful, into C:/ users / PC / Desktop / copia_{ blobName }");
        }
    }
}

