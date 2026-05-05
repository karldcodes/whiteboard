using System.Threading.Channels;
using Whiteboard.Domain.Models;
using Whiteboard.Domain.Interfaces;

namespace Whiteboard.Domain;

/* 
* Currently the whiteboard is stored in memory but this could easily be switched out for anything else 
* as we rely only on the interface implementation.
*/
public sealed class WhiteboardStore : IWhiteboardStore
{
    private readonly object _lock = new();
    private readonly WhiteBoard _items = new(PostIts: new List<PostIt>());
    private readonly Channel<QueuedWhiteboardChange> _changes =
        Channel.CreateUnbounded<QueuedWhiteboardChange>();

    public WhiteboardStore()
    {
        // start changes loop that will continuously run and apply updates
        _ = ProcessChangesAsync();
    }

    public WhiteBoard Get()
    {
        lock (_lock)
        {
            return new WhiteBoard(
                PostIts: _items.PostIts.ToList()
            );
        }
    }

    // Queue change commands to the dashboard
    public async Task<ApplyResult> EnqueueChangeAsync(IWhiteboardChange change)
    {
        var completion = new TaskCompletionSource<ApplyResult>(
            TaskCreationOptions.RunContinuationsAsynchronously);

        // queue the change request and a promise to return the result once processed
        await _changes.Writer.WriteAsync(
            new QueuedWhiteboardChange(change, completion));

        return await completion.Task;
    }

    private ApplyResult Apply(IWhiteboardChange change)
    {
        lock (_lock)
        {
            return change.Apply(_items);
        }
    }

    // Process loop that runs forever and applys the commands on the queue
    private async Task ProcessChangesAsync()
    {
        await foreach (var queued in _changes.Reader.ReadAllAsync())
        {
            var result = Apply(queued.Change);       
            queued.Completion.SetResult(result);
        }
    }
}

