namespace Synapse.Example.Test.SynapseTestHelper.Models;

public record PipelineRunResult
{
    public string RunId { get; }

    public string Status { get; }

    public PipelineRunResult(string runId, string status)
    {
        RunId = runId;
        Status = status;
    }
}
