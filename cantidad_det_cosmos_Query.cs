using System;
//using System.IO;
//using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
//using Newtonsoft.Json;
//using Microsoft.Azure.Documents;
//using Microsoft.Azure.Documents.Client;
//using Microsoft.Azure.Documents.Linq;
//using System.Linq;
using System.Collections.Generic;
//using DocumentFormat.OpenXml.Presentation;
//using DocumentFormat.OpenXml.Spreadsheet;
//using NLog.Fluent;
//using Serilog;
//using DocumentFormat.OpenXml.Wordprocessing;
//using System.Drawing.Text;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace get
{
    public static class Driverget
    {
        [FunctionName("GetDrivers")]
        public static IActionResult GetDrivers(
    [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
    [CosmosDB(
            databaseName: "taskDatabase",
            collectionName: "Boleta",
            SqlQuery = "SELECT top 5* FROM c order by c.id desc",

            ConnectionStringSetting = "Connection")]
        IEnumerable<Document> drivers,
    ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            if (drivers is null)
            {
                return new NotFoundResult();
            }

            foreach (var driver in drivers)
            {
                log.LogInformation(driver.Id);
            }

            return new OkObjectResult(drivers);
        }

        public class Document
        {
            public string Id { get; set; }
            public string CodigoEmpresa { get; set; }
            public string NumeroPlanila { get; set; }
            public string NitEmpresa { get; set; }
            public string CodigoDepartamento { get; set; }
            public string NombreDepartamento { get; set; }
            public string NumeroBoleta { get; set; }
            public string FechaPago { get; set; }
            public string CodigoColaborador { get; set; }
            public string NombreColaborador { get; set; }
            public string NitColaborador { get; set; }
            public string FechaInsercion { get; set; }
            public string NombreDocumento { get; set; }
        }




    }
}