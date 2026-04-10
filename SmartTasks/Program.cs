using Scalar.AspNetCore;
using SmartTasks;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();

// 1. Connection to Redis (name 'cache' is in my docker-compose.yml)
var redis = ConnectionMultiplexer.Connect("cache:6379");
builder.Services.AddSingleton<IConnectionMultiplexer>(redis);

// CORS - in development we allow everything
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseCors(); // activate CORS!

if (app.Environment.IsDevelopment())
{
    // generate API file
    app.MapOpenApi();

    // activate Scalar API reference
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("SmartTasks API")
               .WithTheme(ScalarTheme.Moon)
               .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });
}

// 2. Save by IConnectionMultiplexer
app.MapPost("/save", async (TaskModel model, IConnectionMultiplexer redisControl) =>
{
    var db = redisControl.GetDatabase();
    await db.StringSetAsync(model.Name, model.Description);
    return Results.Created($"/list/{model.Name}", model);
});

// 3. Read by IConnectionMultiplexer
app.MapGet("/list/{name}", async (string name, IConnectionMultiplexer redisControl) =>
{
    var db = redisControl.GetDatabase();
    string? value = await db.StringGetAsync(name); // read all even from console
    return value ?? "Value was not found";
});

// 4. The root address returns a list of DB keys
app.MapGet("/", async (IConnectionMultiplexer redisControl) => 
{
    var db = redisControl.GetDatabase();
    var server = redisControl.GetServer("cache", 6379);
    var keys = server.Keys();
    var results = new Dictionary<string, string>();

    foreach (var key in keys)
    {
        var value = await db.StringGetAsync(key);
        results.Add(key.ToString(), value.ToString());
    }

    return results.Any() ? Results.Ok(results) : Results.Content("Redis is empty.");
});

// 5. Delete by IConnectionMultiplexer
app.MapDelete("/delete/{name}", async (string name, IConnectionMultiplexer redisControl) =>
{
    var db = redisControl.GetDatabase();
    bool deleted = await db.KeyDeleteAsync(name);

    return deleted
        ? Results.Ok($"Task '{name}' was deleted.")
        : Results.NotFound("Task not found.");
});

// 6. gets current environment name
app.MapGet("/env", (IWebHostEnvironment env) => env.EnvironmentName);

app.Run();