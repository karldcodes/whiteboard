using Microsoft.AspNetCore.SignalR;
using Whiteboard.Domain.Interfaces;
using Whiteboard.Domain.Commands;
using Whiteboard.Domain.Models;

namespace Whiteboard.Api.Hubs;

public class WhiteboardHub : Hub<IWhiteboardHub>
{
    private readonly IWhiteboardStore _store;

    public WhiteboardHub(IWhiteboardStore store)
    {
        _store = store;
    }

    public async Task AddPostIt(PostIt postIt)
    {
        var result = await _store.EnqueueChangeAsync(new AddPostIt(postIt));
        if (!result.Success)
        {
            await Clients.Caller.PostItConflict(result);
            return;
        }

        postIt.Version = result.CurrentVersion;
        await Clients.Others.PostItAdded(postIt);
    }

    public async Task MovePostIt(Guid postItId, int x, int y, long version)
    {
        var result = await _store.EnqueueChangeAsync(new MovePostIt(postItId, x, y, version));
        if (!result.Success)
        {
            await Clients.Caller.PostItConflict(result);
            return;
        }

        await Clients.Others.PostIdMoved(postItId, x, y, result.CurrentVersion);
    }

    public async Task DeletePostIt(Guid postItId, long version)
    {
        var result = await _store.EnqueueChangeAsync(new DeletePostIt(postItId, version));
        if (!result.Success)
        {
            await Clients.Caller.PostItConflict(result);
            return;
        }

        await Clients.Others.PostItDeleted(postItId);
    }

    public async Task UpdatePostItText(Guid postItId, string text, long version)
    {
        var result = await _store.EnqueueChangeAsync(new UpdatePostItText(postItId, text, version));
        if (!result.Success)
        {
            await Clients.Caller.PostItConflict(result);
            return;
        }

        await Clients.Others.PostItTextUpdated(postItId, text, result.CurrentVersion);
    }
}
