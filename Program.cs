using Microsoft.EntityFrameworkCore;
using NGO_WebAPI_Backend.Models;

var builder = WebApplication.CreateBuilder(args);

// 添加服務到容器中
// 了解更多關於配置 OpenAPI 的資訊：https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// 添加控制器支援
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

// 配置 Entity Framework 和資料庫連線
builder.Services.AddDbContext<NgoplatformDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 添加 CORS 支援（根據環境決定允許的來源）
if (builder.Environment.IsDevelopment())
{
    // 開發環境：允許所有來源（方便開發）
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
    });
}
else
{
    // 生產環境：只允許特定前端網址
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", builder =>
        {
            builder.WithOrigins(
                    "https://happy-wave-01bfc3a00.2.azurestaticapps.net",     // 生產環境前端網址
                    "https://www.happy-wave-01bfc3a00.2.azurestaticapps.net"  // 如果有 www 版本
                )
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials(); // 如果需要傳送 cookies
        });
    });
}

var app = builder.Build();

// 配置 HTTP 請求管道
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// 啟用 CORS
app.UseCors("AllowAll");

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// 添加預設路由－檢查用
app.MapGet("/", () => "NGO API 運作正常 - " + DateTime.Now.ToString());

// 控制器路由啟動
app.MapControllers();

app.Run();
