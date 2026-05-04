using Whiteboard.Domain.Models;
namespace Whiteboard.Domain.Interfaces;

public interface IWhiteboardChange
{
    ApplyResult Apply(WhiteBoard board);
}