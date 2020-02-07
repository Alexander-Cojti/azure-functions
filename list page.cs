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
using System.Linq;

namespace BloApi
{
    public static class blobOperations
    {
        [FunctionName("ListBlob")]
        public static async Task ListBlobsFlatListingAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "List/{segmentSize}")] HttpRequest req, int? segmentSize)
        {
            StorageCredentials storageCredentials = new StorageCredentials("jfkstorageqymjpzort5hho",
                 "CamEKgqVaylmQta0aUTqN8vUy/xvUewVYZOrBvF1mh85zlgtj3fm7YheYRgoB6paMp+VcFUpTRIlow2VHlyCww==");
            CloudStorageAccount storageAccount = new CloudStorageAccount(storageCredentials, useHttps: true);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("datum");


            // List blobs to the console window.
            Console.WriteLine("List blobs in segments (flat listing):");
            Console.WriteLine();

            int i = 0;
            BlobContinuationToken continuationToken = null;
            BlobResultSegment resultSegment = null;

            try
            {
                // Call ListBlobsSegmentedAsync and enumerate the result segment returned, while the continuation token is non-null.
                // When the continuation token is null, the last segment has been returned and execution can exit the loop.
                do
                {
                    // This overload allows control of the segment size. You can return all remaining results by passing null for the maxResults parameter, 
                    // or by calling a different overload.
                    // Note that requesting the blob's metadata as part of the listing operation 
                    // populates the metadata, so it's not necessary to call FetchAttributes() to read the metadata.
                    resultSegment = await container.ListBlobsSegmentedAsync(string.Empty, true, BlobListingDetails.Metadata, segmentSize, continuationToken, null, null);
                    if (resultSegment.Results.Count() > 0)
                    {
                        Console.WriteLine("Page {0}:", ++i);
                    }

                    foreach (var blobItem in resultSegment.Results)
                    {
                        Console.WriteLine("************************************");
                        Console.WriteLine(blobItem.Uri);

                        // A flat listing operation returns only blobs, not virtual directories.
                        // Write out blob properties and metadata.
                        if (blobItem is CloudBlob)
                        {
                            PrintBlobPropertiesAndMetadata((CloudBlob)blobItem);
                        }
                    }

                    Console.WriteLine();

                    continuationToken = resultSegment.ContinuationToken;

                } while (continuationToken != null);
            }
            catch (StorageException e)
            {
                Console.WriteLine(e.Message);
                Console.ReadLine();
                throw;
            }



        }
        private static void PrintBlobPropertiesAndMetadata(CloudBlob blob)
        {
            // Write out properties that are common to all blob types.
            Console.WriteLine();
            Console.WriteLine("-----Blob Properties-----");
            Console.WriteLine("\t Name: {0}", blob.Name);
            Console.WriteLine("\t Container: {0}", blob.Container.Name);
            Console.WriteLine("\t BlobType: {0}", blob.Properties.BlobType);
            Console.WriteLine("\t IsSnapshot: {0}", blob.IsSnapshot);

            // If the blob is a snapshot, write out snapshot properties.
            if (blob.IsSnapshot)
            {
                Console.WriteLine("\t SnapshotTime: {0}", blob.SnapshotTime);
                Console.WriteLine("\t SnapshotQualifiedUri: {0}", blob.SnapshotQualifiedUri);
            }

            Console.WriteLine("\t LeaseState: {0}", blob.Properties.LeaseState);

            // If the blob has been leased, write out lease properties.
            if (blob.Properties.LeaseState != LeaseState.Available)
            {
                Console.WriteLine("\t LeaseDuration: {0}", blob.Properties.LeaseDuration);
                Console.WriteLine("\t LeaseStatus: {0}", blob.Properties.LeaseStatus);
            }

            Console.WriteLine("\t CacheControl: {0}", blob.Properties.CacheControl);
            Console.WriteLine("\t ContentDisposition: {0}", blob.Properties.ContentDisposition);
            Console.WriteLine("\t ContentEncoding: {0}", blob.Properties.ContentEncoding);
            Console.WriteLine("\t ContentLanguage: {0}", blob.Properties.ContentLanguage);
            Console.WriteLine("\t ContentMD5: {0}", blob.Properties.ContentMD5);
            Console.WriteLine("\t ContentType: {0}", blob.Properties.ContentType);
            Console.WriteLine("\t ETag: {0}", blob.Properties.ETag);
            Console.WriteLine("\t LastModified: {0}", blob.Properties.LastModified);
            Console.WriteLine("\t Length: {0}", blob.Properties.Length);

            // Write out properties specific to blob type.
            switch (blob.BlobType)
            {
                case BlobType.AppendBlob:
                    CloudAppendBlob appendBlob = blob as CloudAppendBlob;
                    Console.WriteLine("\t AppendBlobCommittedBlockCount: {0}", appendBlob.Properties.AppendBlobCommittedBlockCount);
                    Console.WriteLine("\t StreamWriteSizeInBytes: {0}", appendBlob.StreamWriteSizeInBytes);
                    break;
                case BlobType.BlockBlob:
                    CloudBlockBlob blockBlob = blob as CloudBlockBlob;
                    Console.WriteLine("\t StreamWriteSizeInBytes: {0}", blockBlob.StreamWriteSizeInBytes);
                    break;
                case BlobType.PageBlob:
                    CloudPageBlob pageBlob = blob as CloudPageBlob;
                    Console.WriteLine("\t PageBlobSequenceNumber: {0}", pageBlob.Properties.PageBlobSequenceNumber);
                    Console.WriteLine("\t StreamWriteSizeInBytes: {0}", pageBlob.StreamWriteSizeInBytes);
                    break;
                default:
                    break;
            }

            Console.WriteLine("\t StreamMinimumReadSizeInBytes: {0}", blob.StreamMinimumReadSizeInBytes);
            Console.WriteLine();

            // Enumerate the blob's metadata.
            Console.WriteLine("Blob metadata:");
            foreach (var metadataItem in blob.Metadata)
            {
                Console.WriteLine("\tKey: {0}", metadataItem.Key);
                Console.WriteLine("\tValue: {0}", metadataItem.Value);
            }



        }
    }
}
