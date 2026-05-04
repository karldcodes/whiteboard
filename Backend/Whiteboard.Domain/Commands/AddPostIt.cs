using Whiteboard.Domain.Interfaces;
using Whiteboard.Domain.Models;

namespace Whiteboard.Domain.Commands;

public sealed record AddPostIt(
    PostIt NewPostIt) : IWhiteboardChange
{
    public ApplyResult Apply(WhiteBoard board)
    {
        // prevent duplicate IDs
        if (board.PostIts.Any(p => p.Id == NewPostIt.Id))
        {
            return ApplyResult.Conflict(
                currentVersion: null,
                message: "Post-it with this ID already exists.");
        }

        NewPostIt.Version = 1;

        board.PostIts.Add(NewPostIt);

        return ApplyResult.Succeeded(NewPostIt.Version);
    }
}