namespace Synapse.Example.Test;

using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SynapseTestHelper;
using Microsoft.Data.SqlClient;
using Azure.Identity;
using System.Linq;
using System.Data;
using System.Collections.Generic;

[TestClass]
public class PipelineTests
{
    private PipelineHelper pipelineHelper;
    private SqlHelper sqlHelper;
    private StorageHelper storageHelper;

    public PipelineTests()
    {
        pipelineHelper = new PipelineHelper("https://rub-synapsedemo-dev-syn.dev.azuresynapse.net");
        sqlHelper = new SqlHelper("https://rub-synapsedemo-dev-syn.dev.azuresynapse.net");
        storageHelper = new StorageHelper("https://rubsynapsedemodevst.blob.core.windows.net");
    }

    [TestMethod]
    public async Task When_unzipping_Expect_file_unzipped_and_zip_removed()
    {
        // arrange : clean previous results and prep with test file
        await storageHelper.UploadFileAsync("mycontainer", "zipped", "test.zip");
        await storageHelper.DeleteBlobIfExistsAsync("mycontainer", "unzipped", "unzipped.txt");

        // act : run pl_unzip pipeline
        var result = await pipelineHelper.RunAndAwaitAsync("pl_unzip", TimeSpan.FromMinutes(10));

        // assert : check that the file is unzipped and the zip file is removed
        var unzippedExists = await storageHelper.BlobExistsAsync("mycontainer", "unzipped", "unzipped.txt");
        var zipExists = await storageHelper.BlobExistsAsync("mycontainer", "zipped", "test.zip");

        Assert.AreEqual("Succeeded", result.Status);
        Assert.IsTrue(unzippedExists);
        Assert.IsFalse(zipExists);
    }

    [TestMethod]
    public async Task When_appending_to_delta_lake_Expect_new_lines_in_external_table()
    {
        string uniqueUnixTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();

        // arrange : upload test file with unique value to expect in assert
        await storageHelper.UploadFileAsync("mycontainer", "", "mytest.json", new BinaryData(new
        {
            ColumnName = "Edition",
            ColumnValue = uniqueUnixTime,
            Ids = "9782503574318,9782503585390",
            User = "AutomatedTest"
        }));

        // act : run pl_json_append_delta_lake pipeline
        var parameters = new Dictionary<string, object>() { ["triggerFile"] = "mytest.json" };
        var result = await pipelineHelper.RunAndAwaitAsync("pl_json_append_delta_lake", TimeSpan.FromMinutes(10), parameters);

        // assert: check that new lines are in external table
        string sql = @$"
            SELECT Edition, Notes FROM [default].[dbo].[sat_topics]
            WHERE ISBN IN ('9782503574318', '9782503574318')
            AND Edition = '{uniqueUnixTime}'
            AND Notes = 'AutomatedTest'
        ";
        var table = sqlHelper.RetrieveQueryResults("DataExplorationDB", sql);

        Assert.AreEqual("Succeeded", result.Status);
        Assert.AreEqual(2, table.Rows.Count);
    }
}