using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System.Net;
using System.Text;

namespace azure_learn.StorageAccount
{
    public class StorageAccount
    {
        private readonly string _connectionString;

        public StorageAccount()
        {
            _connectionString = KeyVault.KeyVault.GetSecret("StorageAccountConnectionString");
        }

        public async Task Exec()
        {
            await Case1();
            await Case2();
            await Case3();
        }

        private async Task Case1()
        {
            //var serviceClient = GetBlobServiceClient(new Uri("https://sttsucchitest1.blob.core.windows.net/"));
            var serviceClient = GetBlobServiceClient(_connectionString);
            var containerClient = GetBlobContainerClient(serviceClient, "test1");
            var blobClient = GetBlobClient(containerClient, "test.txt");
            DumpBlobMetaData(blobClient);

            // download blob
            var downloadResult = await blobClient.DownloadContentAsync();
            var text = Encoding.UTF8.GetString(downloadResult.Value.Content.ToArray());
            Console.WriteLine(text);
        }

        private async Task Case2()
        {
            var serviceClient = GetBlobServiceClient(_connectionString);
            var containerClient = await CreateBlobContainerAsync(serviceClient, "test2");
            _ = await CreateBlob(containerClient, "test2_1.text", new MemoryStream(Encoding.UTF8.GetBytes("Hello World!")));
            _ = await CreateBlob(containerClient, "test2_2.text", new MemoryStream(Encoding.UTF8.GetBytes("Hello World!")));
            _ = await CreateBlob(containerClient, "test2_3.text", new MemoryStream(Encoding.UTF8.GetBytes("Hello World!")));
        }

        private async Task Case3()
        {
            var serviceClient = GetBlobServiceClient(_connectionString);
            var containerClient = GetBlobContainerClient(serviceClient, "test2");
            await foreach (var blboItem in containerClient.GetBlobsAsync())
            {
                Console.WriteLine($"{blboItem.Name}");
            }
        }

        private BlobServiceClient GetBlobServiceClient(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString)) throw new ArgumentNullException(nameof(connectionString));
            return new BlobServiceClient(connectionString);
        }

        private BlobServiceClient GetBlobServiceClient(Uri uri)
        {
            if (uri == null) throw new ArgumentNullException(nameof(uri));
            return new BlobServiceClient(uri, new DefaultAzureCredential());
        }

        private BlobContainerClient GetBlobContainerClient(BlobServiceClient serviceClient, string containerName)
        {
            if (serviceClient is null) throw new ArgumentNullException(nameof(serviceClient));
            return serviceClient.GetBlobContainerClient(containerName);
        }

        private BlobClient GetBlobClient(BlobContainerClient containerClient, string blobName)
        {
            if (containerClient is null) throw new ArgumentNullException(nameof(containerClient));
            return containerClient.GetBlobClient(blobName);
        }

        private async Task<BlobContainerClient> CreateBlobContainerAsync(BlobServiceClient serviceClient, string containerName)
        {
            if (serviceClient is null) throw new ArgumentNullException(nameof(serviceClient));
            var containerExists = serviceClient.GetBlobContainers().FirstOrDefault(c => c.Name == containerName) is not null;
            if (containerExists) return GetBlobContainerClient(serviceClient, containerName);
            return await serviceClient.CreateBlobContainerAsync(containerName);
        }

        private async Task<BlobContentInfo> CreateBlob(BlobContainerClient containerClient, string blobName, Stream stream)
        {
            if (containerClient is null) throw new ArgumentNullException(nameof(containerClient));
            var blobExists = containerClient.GetBlobs().FirstOrDefault(b => b.Name == blobName) is not null;
            if (blobExists)
            {
                var res = containerClient.DeleteBlob(blobName);
                if (res.Status >= 300) throw new HttpRequestException(Encoding.UTF8.GetString(res.Content.ToArray()), null, (HttpStatusCode)res.Status);
            }

            var result = await containerClient.UploadBlobAsync(blobName, stream, CancellationToken.None);
            return result.Value;
        }

        private void DumpBlobMetaData(BlobClient blobClient)
        {
            if (blobClient is null) return;
            Console.WriteLine($"{nameof(blobClient.Uri)}:{blobClient.Uri}");
            Console.WriteLine($"{nameof(blobClient.Name)}:{blobClient.Name}");
            Console.WriteLine($"{nameof(blobClient.AccountName)}:{blobClient.AccountName}");
            Console.WriteLine($"{nameof(blobClient.BlobContainerName)}:{blobClient.BlobContainerName}");
        }
    }
}
