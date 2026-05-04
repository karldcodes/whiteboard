using Whiteboard.Domain.Commands;
using Whiteboard.Domain.Models;

namespace Whiteboard.Tests;

// MethodName_WhenCondition_ExpectedResult
public class AddPostItTests
{
    [Fact]
    public void Apply_ConflictOccurs_ReturnsConflictResult()
    {
        var id = new Guid();

        var board = new WhiteBoard(new List<PostIt>
        {
            new PostIt
            {
                Id = id
            }
        });
        var add = new AddPostIt(new PostIt
        {
            Id = id
        });

        var result = add.Apply(board);

        Assert.False(result.Success);
    }
}
