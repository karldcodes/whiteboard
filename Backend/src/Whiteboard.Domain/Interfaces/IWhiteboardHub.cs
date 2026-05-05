using Whiteboard.Domain.Models;
namespace Whiteboard.Domain.Interfaces;

public interface IWhiteboardHub
{
    Task PostItAdded(PostIt whiteBoard);
    Task PostIdMoved(Guid postItId, int x, int y, long? version);
    Task PostItDeleted(Guid postItId);
    Task PostItTextUpdated(Guid postItId, string text, long? version);
    Task PostItConflict(ApplyResult result);
}