namespace Synapse.Example.Test.SynapseTestHelper.Models;

using System;
using System.Runtime.Serialization;

[Serializable]
public class ActivityNotFound : Exception
{
    public ActivityNotFound()
    {
    }

    public ActivityNotFound(string? activity) : base($"Activity {activity} not found")
    {
    }

    public ActivityNotFound(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected ActivityNotFound(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}