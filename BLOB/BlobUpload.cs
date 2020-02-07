using Microsoft.Azure.WebJobs;
using System.Linq;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net;
using System.Net.Http.Headers;
using System;
using System.Web.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Build.Framework;

namespace blobtrigger
{
    public static class ImageUploadFunction
    {
        [FunctionName("ImageUploadFunction")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")]HttpRequestMessage req)
        {
            var provider = new MultipartMemoryStreamProvider();
            await req.Content.ReadAsMultipartAsync(provider);
            var files = provider.Contents;
            List<string> uploadsurls = new List<string>();
            foreach (var file in files)
            {
                var fileInfo = file.Headers.ContentDisposition;
                Guid guid = Guid.NewGuid();
                string oldFileName = fileInfo.FileName;
                string newFileName = guid.ToString();
                var fileExtension = oldFileName.Split('.').Last().Replace("\"", "").Trim();
                var fileData = await file.ReadAsByteArrayAsync();
                try
                {
                    //Upload file to azure blob storage method
                    var upload = await UploadFileToStorage(fileData, newFileName + "." + fileExtension);
                    uploadsurls.Add(upload);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return new BadRequestObjectResult("Somthing went wrong.");
                }
            }
            return uploadsurls != null
                  ? (ActionResult)new OkObjectResult(uploadsurls)
                   : new BadRequestObjectResult("Somthing went wrong.");
        }
        public static async Task<string> UploadFileToStorage(byte[] fileStream, string fileName)
        {

            StorageCredentials storageCredentials = new StorageCredentials("jfkstorageqymjpzort5hho", "CamEKgqVaylmQta0aUTqN8vUy/xvUewVYZOrBvF1mh85zlgtj3fm7YheYRgoB6paMp+VcFUpTRIlow2VHlyCww==");
            CloudStorageAccount storageAccount = new CloudStorageAccount(storageCredentials, true);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("datum");
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(fileName);
            await blockBlob.UploadFromByteArrayAsync(fileStream, 0, fileStream.Length);
            blockBlob.Properties.ContentType = "image/jpg";
            await blockBlob.SetPropertiesAsync();
            return blockBlob.Uri.ToString();
            //return await Task.FromResult(true);
        }


    }
}









