using Whiteboard.Domain.Interfaces;
using Whiteboard.Domain.Models;

namespace Whiteboard.Domain.Commands;

public sealed record MovePostIt(
    Guid PostItId,
    int X,
    int Y,
    long ExpectedPostItVersion) : IWhiteboardChange
{
    public ApplyResult Apply(WhiteBoard board)
    {
        var index = board.PostIts.FindIndex(p => p.Id == PostItId);

        if (index == -1)
        {
            return ApplyResult.NotFound();
        }

        var current = board.PostIts[index];

        if (current.Version != ExpectedPostItVersion)
        {
            return ApplyResult.Conflict(
                currentVersion: current.Version,
                message: "This post-it has already changed.");
        }

        current.X = X;
        current.Y = Y;
        current.Version++;

        board.PostIts[index] = current;

        return ApplyResult.Succeeded(
            newVersion: current.Version);
    }
}