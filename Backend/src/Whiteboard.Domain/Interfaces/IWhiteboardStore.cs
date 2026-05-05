using Whiteboard.Domain.Models;

namespace Whiteboard.Domain.Interfaces;

public interface IWhiteboardStore
{
    WhiteBoard Get();
    Task<ApplyResult> EnqueueChangeAsync(IWhiteboardChange change);
}