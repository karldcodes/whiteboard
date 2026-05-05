using Whiteboard.Domain.Interfaces;
using Whiteboard.Domain.Models;

namespace Whiteboard.Domain.Commands;

public sealed record DeletePostIt(
    Guid PostItId,
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

        if (current.Version + 1 == ExpectedPostItVersion)
        {
            board.PostIts.RemoveAt(index);
            // todo this inc to version can most likely be removed
            return ApplyResult.Succeeded(current.Version + 1);
        }

        return ApplyResult.Conflict(
                current.Version,
                "This post-it has already been deleted.",
                current);
    }
}