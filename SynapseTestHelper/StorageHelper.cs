namespace Synapse.Example.Test.SynapseTestHelper;

using System;
using System.IO;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Storage.Blobs;

public class StorageHelper
{
    private const string testFilesFolder = "TestFiles";
    private BlobServiceClient blobServiceClient;

    public StorageHelper(string storageUrl)
    {
        var tokenCredential = new DefaultAzureCredential();
        blobServiceClient = new BlobServiceClient(new Uri(storageUrl), tokenCredential);
    }

    public async Task UploadFileAsync(string container, string folder, string testFile)
    {
        var containerClient = blobServiceClient.GetBlobContainerClient(container);

        using var fileStream = new FileStream(Path.Combine(testFilesFolder, testFile), FileMode.Open);

        await DeleteBlobIfExistsAsync(container, folder, testFile);
        await containerClient.UploadBlobAsync(Path.Combine(folder, testFile), fileStream);
    }

    public async Task DeleteBlobIfExistsAsync(string container, string folder, string blobFile)
    {
        var containerClient = blobServiceClient.GetBlobContainerClient(container);

        await containerClient.DeleteBlobIfExistsAsync(Path.Combine(folder, blobFile));
    }

    public async Task<bool> BlobExistsAsync(string container, string folder, string blobFile)
    {
        var containerClient = blobServiceClient.GetBlobContainerClient(container);

        var blobClient = containerClient.GetBlobClient(Path.Combine(folder, blobFile));

        return await blobClient.ExistsAsync();
    }
}
