using Microsoft.AspNetCore.SignalR;

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


public class WhiteboardStore
{
    private readonly WhiteBoard _items = new(PostIts: new List<PostIt>());

    public WhiteBoard Get()
    {
        return _items;
    }

    public void Set(WhiteBoard whiteBoard)
    {
        _items.PostIts.Clear();
        _items.PostIts.AddRange(whiteBoard.PostIts);
    }
}

public interface IWhiteboardHub
{
    Task RecieveNotification(string UserId, WhiteBoard whiteBoard);
    Task ReceiveMessage(WhiteBoard whiteBoard);
}

public class WhiteboardHub : Hub<IWhiteboardHub>
{
    private readonly WhiteboardStore _store;

    public WhiteboardHub(WhiteboardStore store)
    {
        _store = store;
    }

    public override Task OnConnectedAsync()
    {
        return Clients.All.RecieveNotification($"{Context.ConnectionId} joined the board", _store.Get());
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        return Clients.All.RecieveNotification($"{Context.ConnectionId} left the board", _store.Get());
    }

    public async Task UpdateWhiteBoard(WhiteBoard whiteBoard)
    {
        _store.Set(whiteBoard);
        await Clients.All.ReceiveMessage(whiteBoard);
    }
}

public record PostIt(Guid Id, int X, int Y, int W, int H, string Color, string Label);
public record WhiteBoard(List<PostIt> PostIts);