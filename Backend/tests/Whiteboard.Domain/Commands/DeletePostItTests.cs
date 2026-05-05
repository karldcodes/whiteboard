using Whiteboard.Domain;
using Whiteboard.Domain.Commands;
using Whiteboard.Domain.Models;

public sealed class DeletePostItTests
{
    [Fact]
    public void Apply_WhenPostItDoesNotExist_ReturnsNotFound()
    {
        var board = new WhiteBoard();
        var change = new DeletePostIt(
            Guid.NewGuid(),
            ExpectedPostItVersion: 1);


        var result = change.Apply(board);


        Assert.Equal(ApplyResult.NotFound(), result);
    }

    [Fact]
    public void Apply_WhenExpectedVersionIsNextVersion_RemovesPostIt()
    {
        var postItId = Guid.NewGuid();
        var board = new WhiteBoard();
        board.PostIts.Add(new PostIt
        {
            Id = postItId,
            Label = "text",
            Version = 5
        });
        var change = new DeletePostIt(
            postItId,
            ExpectedPostItVersion: 6);


        change.Apply(board);


        Assert.Empty(board.PostIts);
    }

    [Fact]
    public void Apply_WhenExpectedVersionIsNextVersion_ReturnsSucceededWithExpectedVersion()
    {
        var postItId = Guid.NewGuid();
        var board = new WhiteBoard();
        board.PostIts.Add(new PostIt
        {
            Id = postItId,
            Label = "text",
            Version = 5
        });
        var change = new DeletePostIt(
            postItId,
            ExpectedPostItVersion: 6);


        var result = change.Apply(board);


        Assert.Equal(ApplyResult.Succeeded(6), result);
    }

    [Fact]
    public void Apply_WhenExpectedVersionDoesNotMatch_ReturnsConflict()
    {
        var postItId = Guid.NewGuid();
        var current = new PostIt
        {
            Id = postItId,
            Label = "text",
            Version = 5
        };
        var board = new WhiteBoard();
        board.PostIts.Add(current);
        var change = new DeletePostIt(
            postItId,
            ExpectedPostItVersion: 5);


        var result = change.Apply(board);


        Assert.Equal(
            ApplyResult.Conflict(
                5,
                "This post-it has already been deleted.",
                current),
            result);
        Assert.Single(board.PostIts);
        Assert.Equal(postItId, board.PostIts[0].Id);
        Assert.Equal(5, board.PostIts[0].Version);
    }

    [Fact]
    public void Apply_WhenExpectedVersionDoesNotMatch_DoesNotRemovePostIt()
    {
        var postItId = Guid.NewGuid();
        var board = new WhiteBoard();
        board.PostIts.Add(new PostIt
        {
            Id = postItId,
            Label = "text",
            Version = 5
        });
        var change = new DeletePostIt(
            postItId,
            ExpectedPostItVersion: 99);


        change.Apply(board);


        var postIt = Assert.Single(board.PostIts);
        Assert.Equal(postItId, postIt.Id);
        Assert.Equal("text", postIt.Label);
        Assert.Equal(5, postIt.Version);
    }
}

