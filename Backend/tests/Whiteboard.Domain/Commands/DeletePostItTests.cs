using Whiteboard.Domain;
using Whiteboard.Domain.Commands;
using Whiteboard.Domain.Models;

public sealed class DeletePostItTests
{
    [Fact]
    public void Apply_WhenPostItDoesNotExist_ReturnsNotFound()
    {
        var board = new WhiteBoard(PostIts: new List<PostIt>());
        var change = new DeletePostIt(
            Guid.NewGuid(),
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
            Label = "text",
            Version = 5
        }});
        var change = new DeletePostIt(
            postItId,
            ExpectedPostItVersion: 4);


        var result = change.Apply(board);


        Assert.Equal(
            ApplyResult.Conflict(
                5,
                "This post-it has already changed or was deleted."),
            result);

        Assert.Single(board.PostIts);
        Assert.Equal(postItId, board.PostIts[0].Id);
        Assert.Equal(5, board.PostIts[0].Version);
    }

    [Fact]
    public void Apply_WhenVersionMatches_RemovesPostIt()
    {
        var postItId = Guid.NewGuid();
        var board = new WhiteBoard(
            PostIts: new List<PostIt> { 
                new PostIt
                {
                    Id = postItId,
                    Label = "text",
                    Version = 5
                }
            }
        );
        var change = new DeletePostIt(
            postItId,
            ExpectedPostItVersion: 5);


        change.Apply(board);


        Assert.Empty(board.PostIts);
    }

    [Fact]
    public void Apply_WhenVersionMatches_RemovesOnlyMatchingPostIt()
    {
        var postItId = Guid.NewGuid();
        var otherPostItId = Guid.NewGuid();
        var board = new WhiteBoard(
            PostIts: new List<PostIt> { 
                new PostIt
                {
                    Id = otherPostItId,
                    Label = "other",
                    Version = 2
                }
            }
        );
        board.PostIts.Add(new PostIt
        {
            Id = postItId,
            Label = "delete me",
            Version = 5
        });
        var change = new DeletePostIt(
            postItId,
            ExpectedPostItVersion: 5);


        change.Apply(board);


        var remaining = Assert.Single(board.PostIts);
        Assert.Equal(otherPostItId, remaining.Id);
        Assert.Equal("other", remaining.Label);
        Assert.Equal(2, remaining.Version);
    }

    [Fact]
    public void Apply_WhenVersionMatches_ReturnsSucceededWithNextVersion()
    {
        var postItId = Guid.NewGuid();
        var board = new WhiteBoard(
            PostIts: new List<PostIt> { 
                new PostIt
                {
                    Id = postItId,
                    Label = "text",
                    Version = 5
                }
            }
        );
        var change = new DeletePostIt(
            postItId,
            ExpectedPostItVersion: 5);


        var result = change.Apply(board);


        Assert.Equal(ApplyResult.Succeeded(6), result);
    }
}