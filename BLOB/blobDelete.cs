using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Auth;
using System;

namespace DeleteBlob
{
    public static class DeleteBlob
    {
        [FunctionName("DeleteBlob")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route ="blob")] HttpRequest req)
        {
            StorageCredentials storageCredentials = new StorageCredentials("jfkstorageqymjpzort5hho", "CamEKgqVaylmQta0aUTqN8vUy/xvUewVYZOrBvF1mh85zlgtj3fm7YheYRgoB6paMp+VcFUpTRIlow2VHlyCww==");
            CloudStorageAccount storageAccount = new CloudStorageAccount(storageCredentials, true);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("datum");
            CloudBlockBlob blockBlob = container.GetBlockBlobReference("hola.json");
            

            await blockBlob.DeleteIfExistsAsync();

            return (ActionResult) new OkObjectResult("blob eliminado!!");
          }
    }
}
