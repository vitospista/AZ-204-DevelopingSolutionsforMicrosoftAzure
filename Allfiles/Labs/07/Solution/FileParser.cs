using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System.Text;

namespace Solution
{
    public static class FileParser
    {
        [FunctionName("FileParser")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            var connString = Environment.GetEnvironmentVariable("StorageConnectionString");

            try
            {
                BlobServiceClient client = new BlobServiceClient(connString);
                BlobContainerClient dropContainer = client.GetBlobContainerClient("drop");
                BlobClient blob = dropContainer.GetBlobClient("records.json");

                using (MemoryStream ms = new MemoryStream())
                {
                    await blob.DownloadToAsync(ms);
                    string content = Encoding.UTF8.GetString(ms.ToArray());
                    return new OkObjectResult(content);
                }
            }
            catch
            {
                return new OkObjectResult("Something terrible just happened");
            }
        }
    }
}
