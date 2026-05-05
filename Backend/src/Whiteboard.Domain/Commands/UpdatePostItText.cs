using Whiteboard.Domain.Interfaces;
using Whiteboard.Domain.Models;

namespace Whiteboard.Domain.Commands;

public sealed record UpdatePostItText(
    Guid PostItId,
    string Text,
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
            // this should be the correct next change sent from FE is one ahead
            current.Label = Text;
            current.Version++;
            board.PostIts[index] = current;
            return ApplyResult.Succeeded(current.Version);
        }
        
        // send postit object too?
        return ApplyResult.Conflict(
            current.Version,
            "This post-it text has already changed.", 
            current);
    }
}