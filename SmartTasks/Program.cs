using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// 1. Connection to Redis (name 'cache' is in my docker-compose.yml)
var redis = ConnectionMultiplexer.Connect("cache:6379");
builder.Services.AddSingleton<IConnectionMultiplexer>(redis);

var app = builder.Build();

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

app.Run();