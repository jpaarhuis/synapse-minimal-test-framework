namespace Synapse.Example.Test.SynapseTestHelper;

using Microsoft.Data.SqlClient;

public class SqlHelper
{
    private const int SQL_TIMEOUT_ERROR_CODE = -2146232060;
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
            catch (SqlException ex) when (ex.ErrorCode == SQL_TIMEOUT_ERROR_CODE)
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
