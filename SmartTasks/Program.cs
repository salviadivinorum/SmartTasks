using StackExchange.Redis;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();

// 1. Connection to Redis (name 'cache' is in my docker-compose.yml)
var redis = ConnectionMultiplexer.Connect("cache:6379");
builder.Services.AddSingleton<IConnectionMultiplexer>(redis);

var app = builder.Build();

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
app.MapGet("/save/{task}", async (string task, IConnectionMultiplexer redisControl) =>
{
    var db = redisControl.GetDatabase();
    await db.StringSetAsync(task, DateTime.Now.ToString()); // Use  StringSetAsync
    return $"Saved: {task}";
});

// 3. Read by IConnectionMultiplexer
app.MapGet("/list/{task}", async (string task, IConnectionMultiplexer redisControl) =>
{
    var db = redisControl.GetDatabase();
    string? value = await db.StringGetAsync(task); // read all even from console
    return value ?? "Value was not found";
});

// 4. The root address returns a list of DB keys
app.MapGet("/", async (IConnectionMultiplexer redisControl) => {
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

// 5. gets current environment name
app.MapGet("/env", (IWebHostEnvironment env) => env.EnvironmentName);

app.Run();