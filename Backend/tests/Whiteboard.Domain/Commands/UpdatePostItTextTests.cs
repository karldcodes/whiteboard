using Whiteboard.Domain;
using Whiteboard.Domain.Commands;
using Whiteboard.Domain.Models;

namespace tests;

public sealed class UpdatePostItTextTests
{
    [Fact]
    public void Apply_WhenPostItDoesNotExist_ReturnsNotFound()
    {
        var board = new WhiteBoard();
        var change = new UpdatePostItText(
            Guid.NewGuid(),
            "new text",
            ExpectedPostItVersion: 1);


        var result = change.Apply(board);


        Assert.Equal(ApplyResult.NotFound(), result);
    }

    [Fact]
    public void Apply_WhenExpectedVersionIsNextVersion_UpdatesText()
    {
        var postItId = Guid.NewGuid();
        var board = new WhiteBoard();
        board.PostIts.Add(new PostIt
        {
            Id = postItId,
            Label = "old text",
            Version = 5
        });

        var change = new UpdatePostItText(
            postItId,
            "new text",
            ExpectedPostItVersion: 6);


        change.Apply(board);


        Assert.Equal("new text", board.PostIts[0].Label);
    }

    [Fact]
    public void Apply_WhenExpectedVersionIsNextVersion_IncrementsVersion()
    {
        var postItId = Guid.NewGuid();
        var board = new WhiteBoard();
        board.PostIts.Add(new PostIt
        {
            Id = postItId,
            Label = "old text",
            Version = 5
        });

        var change = new UpdatePostItText(
            postItId,
            "new text",
            ExpectedPostItVersion: 6);


        change.Apply(board);


        Assert.Equal(6, board.PostIts[0].Version);
    }

    [Fact]
    public void Apply_WhenExpectedVersionIsNextVersion_ReturnsSucceededWithNewVersion()
    {
        var postItId = Guid.NewGuid();
        var board = new WhiteBoard();
        board.PostIts.Add(new PostIt
        {
            Id = postItId,
            Label = "old text",
            Version = 5
        });

        var change = new UpdatePostItText(
            postItId,
            "new text",
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
            Label = "old text",
            Version = 5
        };

        var board = new WhiteBoard();
        board.PostIts.Add(current);

        var change = new UpdatePostItText(
            postItId,
            "new text",
            ExpectedPostItVersion: 5); // not current+1


        var result = change.Apply(board);


        Assert.Equal(
            ApplyResult.Conflict(
                5,
                "This post-it text has already changed.",
                current),
            result);
        Assert.Equal("old text", board.PostIts[0].Label);
        Assert.Equal(5, board.PostIts[0].Version);
    }

    [Fact]
    public void Apply_WhenExpectedVersionDoesNotMatch_DoesNotUpdatePostIt()
    {
        var postItId = Guid.NewGuid();

        var board = new WhiteBoard();
        board.PostIts.Add(new PostIt
        {
            Id = postItId,
            Label = "old text",
            Version = 5
        });

        var change = new UpdatePostItText(
            postItId,
            "new text",
            ExpectedPostItVersion: 999);


        change.Apply(board);
        

        var postIt = Assert.Single(board.PostIts);
        Assert.Equal("old text", postIt.Label);
        Assert.Equal(5, postIt.Version);
    }
}