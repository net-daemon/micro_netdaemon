using System.Diagnostics.CodeAnalysis;

namespace MicroHomeAssistantClient.Common.Exceptions;

// We allow exception not to have default constructors since
// in this case it only makes sense to have a reason and a message
[SuppressMessage("", "RCS1194")]
public class HaConncetionException : Exception
{
    public HaConncetionException(string message) : base(
        message)
    {
        Reason = HaDisconnectReason.Error;
    }
    
    public HaConncetionException(HaDisconnectReason reason) : base(
        $"Home assistant disconnected reason:{reason}")
    {
        Reason = reason;
    }

    public HaDisconnectReason Reason { get; } 
}