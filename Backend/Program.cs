using Microsoft.AspNetCore.SignalR;
using Whiteboard.Domain;
using Whiteboard.Domain.Interfaces;
using Whiteboard.Domain.Commands;
using Whiteboard.Domain.Models;

var builder = WebApplication.CreateBuilder(args);

const string frontentOrigin = "frontendOrigin";

// setup cors policy so that only the frontend domain can access the backend services
builder.Services.AddCors(options =>
{
    options.AddPolicy(frontentOrigin,
    policy =>
    {
        policy.WithOrigins("http://localhost:5173")
                            .AllowAnyHeader()
                            .AllowAnyMethod()
                            .AllowCredentials();
    });
});

// Add services
builder.Services.AddOpenApi();
builder.Services.AddSignalR();
builder.Services.AddSingleton<WhiteboardStore>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapHub<WhiteboardHub>("/whiteboardHub");

app.MapGet("/", () =>
{
    return "backend running";
});

app.UseCors(frontentOrigin);

app.Run();




public class WhiteboardHub : Hub<IWhiteboardHub>
{
    private readonly WhiteboardStore _store;

    public WhiteboardHub(WhiteboardStore store)
    {
        _store = store;
    }

    public async Task AddPostIt(PostIt postIt)
    {
        var result = await _store.EnqueueChangeAsync(new AddPostIt(postIt));
        if (!result.Success)
        {
            await Clients.Caller.PostItConflict(result);
            return;
        }

        postIt.Version = result.CurrentVersion;
        await Clients.Others.PostItAdded(postIt);
    }

    public async Task MovePostIt(Guid postItId, int x, int y, long version)
    {
        var result = await _store.EnqueueChangeAsync(new MovePostIt(postItId, x, y, version));
        if (!result.Success)
        {
            await Clients.Caller.PostItConflict(result);
            return;
        }

        await Clients.Others.PostIdMoved(postItId, x, y, result.CurrentVersion);
    }

    public async Task DeletePostIt(Guid postItId, long version)
    {
        var result = await _store.EnqueueChangeAsync(new DeletePostIt(postItId, version));
        if (!result.Success)
        {
            await Clients.Caller.PostItConflict(result);
            return;
        }

        await Clients.Others.PostItDeleted(postItId);
    }

    public async Task UpdatePostItText(Guid postItId, string text, long version)
    {
        var result = await _store.EnqueueChangeAsync(new UpdatePostItText(postItId, text, version));
        if (!result.Success)
        {
            await Clients.Caller.PostItConflict(result);
            return;
        }

        await Clients.Others.PostItTextUpdated(postItId, text, result.CurrentVersion);
    }
}
