namespace Synapse.Example.Test.SynapseTestHelper;

using System;
using System.IO;
using System.Linq;
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

    public async Task UploadFileAsync(string container, string folder, string fileName, BinaryData data)
    {
        var containerClient = blobServiceClient.GetBlobContainerClient(container);

        await DeleteBlobIfExistsAsync(container, folder, fileName);
        await containerClient.UploadBlobAsync(Path.Combine(folder, fileName), data);
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

    public async Task ReplaceFolderAsync(string container, string remoteFolder, string localFolder)
    {
        var containerClient = blobServiceClient.GetBlobContainerClient(container);

        await DeleteFolderAsync(container, remoteFolder);

        var localTestFilesFolder = Path.Combine(testFilesFolder, localFolder);
        string[] filePaths = Directory.GetFiles(localTestFilesFolder, "*", SearchOption.AllDirectories);

        foreach (var filePath in filePaths)
        {
            string relativePath = Path.GetRelativePath(localTestFilesFolder, filePath);
            var blobClient = containerClient.GetBlobClient(Path.Combine(remoteFolder, relativePath));
            await blobClient.UploadAsync(filePath, true);
        }
    }

    public async Task DeleteFolderAsync(string container, string folder)
    {
        var containerClient = blobServiceClient.GetBlobContainerClient(container);
        var blobs = containerClient.GetBlobsAsync(prefix: folder).AsPages();

        await foreach (var page in blobs)
        {
            foreach (var blob in page.Values.Where(blob => blob.Properties.ContentLength > 0))
            {
                await containerClient.DeleteBlobIfExistsAsync(blob.Name);
            }
        }
    }
}
