using Whiteboard.Domain;
using Whiteboard.Domain.Commands;
using Whiteboard.Domain.Models;

namespace tests;

public class UpdatePostItTextTests
{
    [Fact]
    public void Apply_WhenPostItDoesNotExist_ReturnsNotFound()
    {
        var board = new WhiteBoard(PostIts: new List<PostIt>());
        var change = new UpdatePostItText(
            Guid.NewGuid(),
            "new text",
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
                    Label = "old text",
                    Version = 5
                }
            }
        );
        var change = new UpdatePostItText(
            postItId,
            "new text",
            ExpectedPostItVersion: 4);


        var result = change.Apply(board);
        

        Assert.Equal(
            ApplyResult.Conflict(
                5,
                "This post-it text has already changed."),
            result);
        Assert.Equal("old text", board.PostIts[0].Label);
        Assert.Equal(5, board.PostIts[0].Version);
    }

    [Fact]
    public void Apply_WhenVersionMatches_UpdatesText()
    {
        var postItId = Guid.NewGuid();
        var board = new WhiteBoard(
            PostIts: new List<PostIt> {
                new PostIt
        {
            Id = postItId,
            Label = "old text",
            Version = 5
        }});
        var change = new UpdatePostItText(
            postItId,
            "new text",
            ExpectedPostItVersion: 5);


        change.Apply(board);


        Assert.Equal("new text", board.PostIts[0].Label);
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
                    Label = "old text",
                    Version = 5
                }
            }
        );
        var change = new UpdatePostItText(
            postItId,
            "new text",
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
                        Label = "old text",
                        Version = 5
                    }
            }
        );
        var change = new UpdatePostItText(
            postItId,
            "new text",
            ExpectedPostItVersion: 5);


        var result = change.Apply(board);


        Assert.Equal(ApplyResult.Succeeded(6), result);
    }
}
