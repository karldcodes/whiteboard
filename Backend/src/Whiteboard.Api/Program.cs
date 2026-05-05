using Whiteboard.Domain.Store;
using Whiteboard.Domain.Interfaces;
using Whiteboard.Api.Hubs;

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
builder.Services.AddSingleton<IWhiteboardStore, WhiteboardStore>();

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