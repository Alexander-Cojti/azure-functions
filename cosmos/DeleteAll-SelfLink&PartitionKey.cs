using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using System.Linq;
using OfficeOpenXml.FormulaParsing.LexicalAnalysis;
using System.Collections.Generic;
using System.Diagnostics;

namespace get
{
    public class Cosmos
    {


        [FunctionName("DeleteAll")]
        public async Task GetRecords(
       [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = null)] HttpRequest req,
       [CosmosDB(databaseName: "taskDatabase", collectionName: "Boleta", ConnectionStringSetting = "Connection")]DocumentClient client)
        {
            try
            {
                var db = client.CreateDatabaseQuery().ToList().First();
                var coll = client.CreateDocumentCollectionQuery(db.CollectionsLink).ToList().First();
                var docs = client.CreateDocumentQuery(coll.DocumentsLink);
                foreach (var doc in docs)
                {

                    await client.DeleteDocumentAsync(doc.SelfLink, new RequestOptions { PartitionKey = new Microsoft.Azure.Documents.PartitionKey(doc.Id) });
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);    
                throw;
            }
        }
    }
}