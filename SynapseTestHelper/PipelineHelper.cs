namespace Synapse.Example.Test.SynapseTestHelper;

using Azure.Analytics.Synapse.Artifacts;
using Azure.Analytics.Synapse.Artifacts.Models;

public class PipelineHelper
{
    private const int pollingIntervalSeconds = 2;
    private readonly PipelineClient pipelineClient;
    private readonly PipelineRunClient pipelineRunClient;

    public PipelineHelper(string synapseDevUrl)
    {
        var tokenCredential = new DefaultAzureCredential();

        pipelineClient = new PipelineClient(new Uri(synapseDevUrl), tokenCredential);
        pipelineRunClient = new PipelineRunClient(new Uri(synapseDevUrl), tokenCredential);
    }

    public async Task<PipelineRunResult> RunAndAwaitAsync(string pipelineName, TimeSpan timeout, IDictionary<string, object>? parameters = null)
    {
        if (timeout.TotalMinutes > 120 || timeout.TotalMinutes < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(timeout), "Timeout should be between 0 and 120 minutes");
        }

        var response = await pipelineClient.CreatePipelineRunAsync(pipelineName, parameters: parameters);
        string runId = response.Value.RunId;

        Response<PipelineRun> run;

        var retries = timeout.TotalSeconds / pollingIntervalSeconds;

        do
        {
            await Task.Delay(TimeSpan.FromSeconds(pollingIntervalSeconds));
            run = await pipelineRunClient.GetPipelineRunAsync(runId);
        } while (retries == 0 
            || run.Value.Status == "Queued" 
            || run.Value.Status == "InProgress" 
            || run.Value.Status == "Canceling");

        return new PipelineRunResult(runId, run.Value.Status);
    }

    public async Task<IReadOnlyList<ActivityRun>> GetRunActivitiesAsync(string runId)
    {
        var run = await pipelineRunClient.GetPipelineRunAsync(runId);

        var runFilterParameters = new RunFilterParameters(DateTimeOffset.Now.AddDays(-1), DateTimeOffset.Now);
        var result = await pipelineRunClient.QueryActivityRunsAsync(run.Value.PipelineName, runId, runFilterParameters);

        return result.Value.Value;
    }

    public async Task<IDictionary<string, object>?> GetActivityInputAsync(string runId, string activityName)
    {
        var activities = await GetRunActivitiesAsync(runId);
        var activity = activities.FirstOrDefault(a => a.ActivityName == "CopyEachFile");

        if (activity == null) throw new ActivityNotFound("activityName");

        return activity.Input as IDictionary<string, object>;
    }
}