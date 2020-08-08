using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Azure.Storage.Blobs.Models;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration.UserSecrets;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace BlobManager
{
    class Program
    {
        private const string connStr = "AZURE_STORAGE_CONNECTION_STRING";
        private const string accountName = "AZURE_STORAGE_ACCOUNT_NAME";
        private const string accountKey = "AZURE_STORAGE_ACCOUNT_KEY";

        private static IDictionary<string, string> _secrets = new Dictionary<string, string>(3);

        static async Task Main(string[] args)
        {
            var config = new ConfigurationBuilder().AddUserSecrets<Program>().Build();

            foreach (string secret in new[] { connStr, accountName, accountKey })
            {
                GetSecret(config, secret);
            }

            BlobServiceClient client = new BlobServiceClient(_secrets[connStr]);

            AccountInfo info = await client.GetAccountInfoAsync();

            await Console.Out.WriteLineAsync($"Account kind:\t{info?.AccountKind}");
            await Console.Out.WriteLineAsync($"Account sku:\t{info?.SkuName}");

            await EnumerateContainersAsync(client);

            BlobContainerClient rasterContainer = await GetContainerAsync(client, "raster-graphics");
            BlobClient blob = await GetBlobAsync(rasterContainer, "graph.jpg");

            string path = @"C:\Users\vitos\Desktop\Work\Azure Course\AZ-204-DevelopingSolutionsforMicrosoftAzure\Allfiles\Labs\03\Starter\Images\graph.svg";
            BlobContainerClient vectorContainer = await GetContainerAsync(client, "vector-graphics");
            string svgBlobName = "graph.svg";
            using (Stream contentStream = new FileStream(path, FileMode.Open))
            {
                var uploadResult = await UploadBlobAsync(vectorContainer, svgBlobName, contentStream);
            }

            BlobClient svgBlob = await GetBlobAsync(vectorContainer, svgBlobName);
        }

        private static void GetSecret(IConfigurationRoot configuration, string secretKey)
        {
            var secretProvider = configuration.Providers.First();
            string secretValue;
            if (!secretProvider.TryGet(secretKey, out secretValue)) return;
            _secrets[secretKey] = secretValue;
        }

        private static async Task<BlobClient> GetBlobAsync(BlobContainerClient containerClient, string blobName)
        {
            var accessPolicy = await containerClient.GetAccessPolicyAsync();

            var blobClient = containerClient.GetBlobClient(blobName);

            string sasToken = string.Empty;

            if (accessPolicy.Value.BlobPublicAccess == PublicAccessType.None)
            {
                sasToken = GetSasToken(blobName, blobClient);
            }

            Console.WriteLine($"Url of blob: {blobClient.Uri}?{sasToken}");

            return blobClient;
        }

        private static string GetSasToken(string blobName, BlobClient blobClient)
        {
            BlobSasBuilder builder = new BlobSasBuilder()
            {
                BlobContainerName = blobClient.BlobContainerName,
                BlobName = blobName,
                Resource = "b",
                StartsOn = DateTimeOffset.UtcNow.AddMinutes(-15),
                ExpiresOn = DateTimeOffset.UtcNow.AddHours(1),
            };
            builder.SetPermissions(BlobSasPermissions.Read);

            StorageSharedKeyCredential storageSharedKeyCredential = new StorageSharedKeyCredential(_secrets[accountName], _secrets[accountKey]);
            return builder.ToSasQueryParameters(storageSharedKeyCredential).ToString();
        }

        private static async Task<int> UploadBlobAsync(BlobContainerClient containerClient, string blobname, Stream content)
        {
            //check if blob already exists:
            var blobClient = containerClient.GetBlobClient(blobname);
            await blobClient.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots);

            var result = await containerClient.UploadBlobAsync(blobname, content);

            Console.WriteLine($"Uploaded blob {blobname}");

            return result.GetRawResponse().Status;
        }

        private static async Task EnumerateContainersAsync(BlobServiceClient client)
        {
            await foreach (var container in client.GetBlobContainersAsync())
            {
                await Console.Out.WriteLineAsync($"Container:\t{container.Name}");
                await EnumerateBlobsAsync(client, container.Name);
            }
        }

        private static async Task EnumerateBlobsAsync(BlobServiceClient client, string containerName)
        {
            BlobContainerClient container = client.GetBlobContainerClient(containerName);

            var accessPolicy = await container.GetAccessPolicyAsync();
            await Console.Out.WriteLineAsync($"Access mode:\t{accessPolicy.Value.BlobPublicAccess}");

            await foreach (var blob in container.GetBlobsAsync())
            {
                await Console.Out.WriteLineAsync($"Existing Blob:\t{blob.Name}");
            }
        }

        private static async Task<BlobContainerClient> GetContainerAsync(BlobServiceClient client, string containerName)
        {
            BlobContainerClient container = client.GetBlobContainerClient(containerName);

            await container.CreateIfNotExistsAsync(PublicAccessType.Blob);
            return container;
        }
    }
}
