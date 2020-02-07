using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Auth;
using System.Collections.Generic;

namespace GetBlob
{
    public static class GetBlob
    {


        [FunctionName("getBlob")]
        public static async Task<List<string>> ListFiles(

            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req)
        {

            StorageCredentials storageCredentials = new StorageCredentials("jfkstorageqymjpzort5hho", "CamEKgqVaylmQta0aUTqN8vUy/xvUewVYZOrBvF1mh85zlgtj3fm7YheYRgoB6paMp+VcFUpTRIlow2VHlyCww==");
            CloudStorageAccount storageAccount = new CloudStorageAccount(storageCredentials, useHttps: true);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("datum");
            List<string> blobs = new List<string>();
            try
            {
                BlobResultSegment resultSegment = await container.ListBlobsSegmentedAsync(null);
                foreach (IListBlobItem item in resultSegment.Results)
                {
                    if (item.GetType() == typeof(CloudBlockBlob))
                    {
                        CloudBlockBlob blob = (CloudBlockBlob)item;
                        blobs.Add(blob.Name);
                    }
                    else if (item.GetType() == typeof(CloudPageBlob))
                    {
                        CloudPageBlob blob = (CloudPageBlob)item;
                        blobs.Add(blob.Name);
                    }
                    else if (item.GetType() == typeof(CloudBlobDirectory))
                    {
                        CloudBlobDirectory dir = (CloudBlobDirectory)item;
                        blobs.Add(dir.Uri.ToString());
                    }
                }

            }
            catch
            {
            }
            return blobs;

        }
    }
}
