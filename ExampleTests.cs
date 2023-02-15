namespace Synapse.Example.Test;

using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SynapseTestHelper;

[TestClass]
public class PipelineTests
{
    private PipelineHelper pipelineHelper;
    private StorageHelper storageHelper;

    public PipelineTests()
    {
        pipelineHelper = new PipelineHelper("https://rub-synapsedemo-dev-syn.dev.azuresynapse.net");
        storageHelper = new StorageHelper("https://rubsynapsedemodevst.blob.core.windows.net");
    }

    [TestMethod]
    public async Task When_unzipping_Expect_file_unzipped_and_zip_removed()
    {
        // arrange : clean previous results and prep with test file
        await storageHelper.UploadFileAsync("mycontainer", "zipped", "test.zip");
        await storageHelper.DeleteBlobIfExistsAsync("mycontainer", "unzipped", "unzipped.txt");

        // act : run pl_unzip pipeline
        var status = await pipelineHelper.RunAndAwaitAsync("pl_unzip", TimeSpan.FromMinutes(10));

        // assert : check that the file is unzipped and the zip file is removed
        var unzippedExists = await storageHelper.BlobExistsAsync("mycontainer", "unzipped", "unzipped.txt");
        var zipExists = await storageHelper.BlobExistsAsync("mycontainer", "zipped", "test.zip");

        Assert.AreEqual("Succeeded", status.Status);
        Assert.IsTrue(unzippedExists);
        Assert.IsFalse(zipExists);
    }
}