using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using VA.Framework.Common.Azure.Storage.Blobs.Definition;

namespace VA.Framework.Common.Azure.Storage.Blobs.Implementation
{
    public class AzureBlobServiceClient : IAzureBlobServiceClient
    {
        private readonly BlobServiceClient _blobServiceClient;

        public AzureBlobServiceClient(string storageAccountConnectionString)
        {
            _blobServiceClient = new BlobServiceClient(storageAccountConnectionString);
        }

        public async Task<Stream> GetBlob(string container, string blobName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(container);
            var blobClient = containerClient.GetBlobClient(blobName);

            if (await blobClient.ExistsAsync())
            {
                var fileStream = await blobClient.DownloadAsync();
                return fileStream.Value.Content;
            }
            else throw new KeyNotFoundException($"Blob {blobName} NOT FOUND");

        }

        public async Task DeleteBlob(string container, string blobName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(container);
            var blobClient = containerClient.GetBlobClient(blobName);
            await blobClient.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots);
        }

        public async Task<bool> ExistsBlob(string container, string blobName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(container);
            var blobClient = containerClient.GetBlobClient(blobName);
            var result = await blobClient.ExistsAsync();
            return result.Value;
        }

        public async Task<bool> MoveBlob(string containerFrom, string blobName, string containerTo)
        {
            var containerFromClient = _blobServiceClient.GetBlobContainerClient(containerFrom);
            var blobClient = containerFromClient.GetBlobClient(blobName);

            if (await blobClient.ExistsAsync())
            {

                var fileStream = await blobClient.DownloadAsync();

                var containerToClient = _blobServiceClient.GetBlobContainerClient(containerTo);
                await containerToClient.UploadBlobAsync(blobName, fileStream.Value.Content);
                await blobClient.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots);
                return true;

            }
            else throw new KeyNotFoundException($"Blob {blobName} NOT FOUND");
        }

        public async Task UploadBlob(string container, string blobName, Stream blob)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(container);
            await containerClient.UploadBlobAsync(blobName, blob);
        }
    }
}
