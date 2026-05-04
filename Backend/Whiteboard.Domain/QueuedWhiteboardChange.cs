using Whiteboard.Domain.Interfaces;
namespace Whiteboard.Domain;
public sealed record QueuedWhiteboardChange(
    IWhiteboardChange Change,
    TaskCompletionSource<ApplyResult> Completion);

