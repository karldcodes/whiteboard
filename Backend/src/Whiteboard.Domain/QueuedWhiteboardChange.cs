using Whiteboard.Domain.Interfaces;
namespace Whiteboard.Domain;

/*
* Helper so that when a change is applied we can store the return in a promise 
* that when it resolves returns a ApplyResult object
*/
public sealed record QueuedWhiteboardChange(
    IWhiteboardChange Change,
    TaskCompletionSource<ApplyResult> Completion);

