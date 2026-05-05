using Whiteboard.Domain.Commands;
using Whiteboard.Domain.Models;

namespace tests;

public class AddPostItTests
{
    [Fact]
    public void Apply_PostIdAlreadyExists_ReturnsConflict()
    {
        var id = Guid.NewGuid();
        var addPostItCommand = new AddPostIt(new PostIt
        {
            Id = id
        });

        var whiteBoard = new WhiteBoard
        {
            PostIts = new List<PostIt>
            {
                new PostIt
                {
                    Id = id
                }
            }
        };


        var result = addPostItCommand.Apply(whiteBoard);


        Assert.False(result.Success);
        Assert.NotNull(result.Error);
    }

    [Fact]
    public void Apply_NoConflicts_ReturnsSuccess()
    {
        var addPostItCommand = new AddPostIt(new PostIt
        {
            Id = Guid.Empty
        });
        var whiteBoard = new WhiteBoard
        {
            PostIts = new List<PostIt>
            {
                new PostIt
                {
                    Id = Guid.NewGuid()
                }
            }
        };


        var result = addPostItCommand.Apply(whiteBoard);


        Assert.True(result.Success);
        Assert.True(result.CurrentVersion == 1);
    }
}
