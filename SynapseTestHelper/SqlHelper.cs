namespace Synapse.Example.Test.SynapseTestHelper;

using Azure.Identity;
using Microsoft.Data.SqlClient;
using System;
using System.Data;

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
        
        using var connection = new SqlConnection(sqlConnectionString);
        connection.Open();

        var command = new SqlCommand(sql, connection);
        var adapter = new SqlDataAdapter(command);

        var ds = new DataSet();
        adapter.Fill(ds);

        return ds.Tables[0];
    }
}
