using Microsoft.EntityFrameworkCore;
using NGO_WebAPI_Backend.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// 添加控制器支持
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

// 配置Entity Framework和数据库连接
builder.Services.AddDbContext<MyDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 添加CORS支持（为了前端能访问API）
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// 启用CORS
app.UseCors("AllowAll");

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

// 測試資料庫連接的API端點
app.MapGet("/test-database", async (MyDbContext context) =>
{
    try
    {
        // 測試資料庫連接
        var canConnect = await context.Database.CanConnectAsync();
        
        if (canConnect)
        {
            return Results.Ok(new { 
                message = "資料庫連接成功！", 
                server = "ngosqlserver.database.windows.net",
                database = "NGOPlatformDB",
                status = "已連接" 
            });
        }
        else
        {
            return Results.BadRequest(new { 
                message = "無法連接到資料庫", 
                server = "ngosqlserver.database.windows.net",
                database = "NGOPlatformDB",
                status = "連接失敗" 
            });
        }
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { 
            message = "資料庫連接錯誤", 
            error = ex.Message,
            server = "ngosqlserver.database.windows.net",
            database = "NGOPlatformDB",
            status = "錯誤" 
        });
    }
})
.WithName("TestDatabase");

// 映射控制器路由
app.MapControllers();

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
