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
        public static string Name { get; private set; }
        public static CloudBlob ICloudBlob { get; private set; }
        public static BlobType BlobType { get; private set; }
        public static long Length { get; private set; }
        public static string ContentType { get; private set; }
        public static DateTimeOffset? LastModified { get; private set; }
        public static DateTimeOffset? SnapshotTime { get; private set; }

        [FunctionName("ListBlob")]
        public static async Task<BlobContinuationToken> ListBlobsFlatListingAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "List/")] HttpRequest req)
        {
            StorageCredentials storageCredentials = new StorageCredentials("jfkstorageqymjpzort5hho",
                 "CamEKgqVaylmQta0aUTqN8vUy/xvUewVYZOrBvF1mh85zlgtj3fm7YheYRgoB6paMp+VcFUpTRIlow2VHlyCww==");
            CloudStorageAccount storageAccount = new CloudStorageAccount(storageCredentials, useHttps: true);
            CloudBlobContainer container = storageAccount.CreateCloudBlobClient().GetContainerReference("datum");

            BlobContinuationToken blobContinuationToken = null;


            BlobContinuationToken token;
            do
            {
                var results = await container.ListBlobsSegmentedAsync(
                   prefix: null,
                   useFlatBlobListing: true,
                   blobListingDetails: BlobListingDetails.None,
                   maxResults: 2,
                   currentToken: blobContinuationToken,
                   options: null,
                   operationContext: null);
                // Get the value of the continuation token returned by the listing call.
                token = results.ContinuationToken;
                foreach (IListBlobItem item in results.Results)
                {
                    List<string> hola = new List<string>
                    {
                        item.Uri.ToString()
                    };
                }

            } while (blobContinuationToken != null);

            return token;

        }
    }
}

