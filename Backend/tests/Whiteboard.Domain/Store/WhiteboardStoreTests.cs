using Whiteboard.Domain.Commands;
using Whiteboard.Domain.Models;
using Whiteboard.Domain.Store;
using Whiteboard.Domain;

namespace tests;

public class WhiteboardStoreTests
{
    [Fact]
public async Task EnqueueChangeAsync_WhenManyConcurrentChangesUseSameVersion_OnlyOneSucceeds()
{
    var store = new WhiteboardStore();
    var postItId = Guid.NewGuid();
    await store.EnqueueChangeAsync(
        new AddPostIt( 
            new PostIt{
                Id = postItId,
                Label = "original",
                X = 0,
                Y = 0
            }
        )
    );
    var tasks = Enumerable.Range(1, 20)
        .Select(i =>
            store.EnqueueChangeAsync(
                new UpdatePostItText(
                    PostItId: postItId,
                    Text: $"update {i}",
                    ExpectedPostItVersion: 2)))
        .ToArray();


    var results = await Task.WhenAll(tasks);


    Assert.Single(results, r => r == ApplyResult.Succeeded(2));
    Assert.Equal(19, results.Count(r => !r.Success));

    var board = store.Get();
    var postIt = Assert.Single(board.PostIts);
    Assert.Equal(2, postIt.Version);
}
}