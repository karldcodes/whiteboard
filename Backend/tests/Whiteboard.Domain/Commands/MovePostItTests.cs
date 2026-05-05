using Whiteboard.Domain;
using Whiteboard.Domain.Commands;
using Whiteboard.Domain.Models;

public sealed class MovePostItTests
{
    [Fact]
    public void Apply_WhenPostItDoesNotExist_ReturnsNotFound()
    {
        var board = new WhiteBoard(PostIts: new List<PostIt>());

        var change = new MovePostIt(
            Guid.NewGuid(),
            X: 10,
            Y: 20,
            ExpectedPostItVersion: 1);

        var result = change.Apply(board);

        Assert.Equal(ApplyResult.NotFound(), result);
    }

    [Fact]
    public void Apply_WhenVersionDoesNotMatch_ReturnsConflict()
    {
        var postItId = Guid.NewGuid();

        var board = new WhiteBoard(
            PostIts: new List<PostIt> { 
                new PostIt
                {
                    Id = postItId,
                    X = 1,
                    Y = 2,
                    Version = 5
                }});

        var change = new MovePostIt(
            postItId,
            X: 10,
            Y: 20,
            ExpectedPostItVersion: 4);

        var result = change.Apply(board);

        Assert.Equal(
            ApplyResult.Conflict(
                currentVersion: 5,
                message: "This post-it has already changed."),
            result);

        Assert.Equal(1, board.PostIts[0].X);
        Assert.Equal(2, board.PostIts[0].Y);
        Assert.Equal(5, board.PostIts[0].Version);
    }

    [Fact]
    public void Apply_WhenVersionMatches_UpdatesPosition()
    {
        var postItId = Guid.NewGuid();

        var board = new WhiteBoard(
            PostIts: new List<PostIt> { 
                new PostIt
                {
                    Id = postItId,
                    X = 1,
                    Y = 2,
                    Version = 5
                }});

        var change = new MovePostIt(
            postItId,
            X: 10,
            Y: 20,
            ExpectedPostItVersion: 5);

        change.Apply(board);

        Assert.Equal(10, board.PostIts[0].X);
        Assert.Equal(20, board.PostIts[0].Y);
    }

    [Fact]
    public void Apply_WhenVersionMatches_IncrementsVersion()
    {
        var postItId = Guid.NewGuid();

        var board = new WhiteBoard(
            PostIts: new List<PostIt> { 
                new PostIt
                {
                    Id = postItId,
                    X = 1,
                    Y = 2,
                    Version = 5
                }});

        var change = new MovePostIt(
            postItId,
            X: 10,
            Y: 20,
            ExpectedPostItVersion: 5);

        change.Apply(board);

        Assert.Equal(6, board.PostIts[0].Version);
    }

    [Fact]
    public void Apply_WhenVersionMatches_ReturnsSucceededWithNewVersion()
    {
        var postItId = Guid.NewGuid();

        var board = new WhiteBoard(
            PostIts: new List<PostIt> { 
                new PostIt
                {
                    Id = postItId,
                    X = 1,
                    Y = 2,
                    Version = 5
                }});

        var change = new MovePostIt(
            postItId,
            X: 10,
            Y: 20,
            ExpectedPostItVersion: 5);

        var result = change.Apply(board);

        Assert.Equal(ApplyResult.Succeeded(6), result);
    }
}