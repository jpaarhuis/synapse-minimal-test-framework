namespace Synapse.Example.Test.SynapseTestHelper;

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Parquet;
using Parquet.Rows;

/// <summary>
/// EXPERIMENTAL. Helper class to write to a delta lake table
/// </summary>
public class DeltaLakeHelper
{
    private string GetLastParquetFilePath(string deltaLakeFolder)
    {
        var deltaLogFiles = Directory.GetFiles(deltaLakeFolder, "_delta_log/*.json", SearchOption.AllDirectories)
            .SelectMany(filePath => File.ReadAllLines(filePath))
            .Select(line => JObject.Parse(line));

        var lastParquetFile = deltaLogFiles
            .Where(logFile => logFile["add"] != null)
            .OrderByDescending(logFile => logFile["add"]["modificationTime"].Value<long>())
            .Select(deltaLog => deltaLog["add"]["path"].Value<string>())
            .FirstOrDefault();

        if (lastParquetFile == null)
        {
            throw new Exception("No parquet files found in delta lake");
        }

        return Path.Combine(deltaLakeFolder, lastParquetFile);
    }

    public async Task WriteDeltaTable(string deltaTablePath, Row[] rows)
    {
        var filePath = this.GetLastParquetFilePath(deltaTablePath);

        var table = await ParquetReader.ReadTableFromFileAsync(filePath);

        var schema = table.Schema;

        table = new Table(schema);
        foreach (var row in rows)
        {
            table.Add(row);
        }

        using Stream outputFileStream = new FileStream(filePath, FileMode.Create);
        using var writer = await ParquetWriter.CreateAsync(schema, outputFileStream);
        await writer.WriteAsync(table);
    }
}
