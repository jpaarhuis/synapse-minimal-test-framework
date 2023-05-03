namespace Synapse.Example.Test.SynapseTestHelper;

using Azure.Identity;
using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Threading.Tasks;

public class SqlHelper
{
    private string ondemandSqlServer;

    public SqlHelper(string synapseDevUrl)
    {
        var client = new Azure.Analytics.Synapse.Artifacts.WorkspaceClient(new Uri(synapseDevUrl), new DefaultAzureCredential());
        var workspace = client.Get();

        ondemandSqlServer = workspace.Value.ConnectivityEndpoints["sqlOnDemand"];
    }

    public DataTable RetrieveQueryResults(string dbName, string sql)
    {
        string sqlConnectionString = $"Server={ondemandSqlServer}; Authentication=Active Directory Default; Encrypt=True; Database={dbName};";

        var ds = new DataSet();

        RetryOnTimeout(() => {
            using var connection = new SqlConnection(sqlConnectionString);
            connection.Open();

            var command = new SqlCommand(sql, connection);
            var adapter = new SqlDataAdapter(command);

            adapter.Fill(ds);
        });
        
        return ds.Tables[0];
    }

    private void RetryOnTimeout(Action sqlMethod)
    {
        var timeout = false;
        var retries = 0;

        do
        {
            if (timeout)
            {
                // wait 60 seconds before retrying
                Task.Delay(60 * 1000).Wait();
            }

            timeout = false;
            retries++;
            try
            {
                sqlMethod();
            }
            catch (SqlException ex) when (ex.Message.Contains("timeout", StringComparison.OrdinalIgnoreCase)) // Error code isn't specific enough, need to determine timeout based on text.
            {
                timeout = true;
            }
        } while (timeout && retries <= 3);

        if (timeout)
        {
            throw new TimeoutException("SQL query timed out");
        }
    }
}
