using Microsoft.EntityFrameworkCore;
using TravelAgencyFrontendAPI.Hubs;
using TravelAgency.Shared.Data;
using TravelAgencyFrontendAPI.Helpers;
using Microsoft.Extensions.FileProviders;

using TravelAgencyFrontendAPI.ECPay.Models; // 引入 ECPayConfiguration
using TravelAgencyFrontendAPI.ECPay.Services; // 引入 ECPayService
using TravelAgencyFrontendAPI.Services;

//using Microsoft.AspNetCore.Authentication.JwtBearer;
//using Microsoft.IdentityModel.Tokens;
//using System.Text;

var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));
// 加入你自己的寄信服務
builder.Services.AddScoped<EmailService>();

//新增 Swagger 設定
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//新增Google登入
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);



// Add services to the container.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("https://localhost:3000", "https://localhost:7107", "https://localhost:7258", "https://192.168.1.122:3000", "https://172.18.132.158:3000", "https://9621-49-159-210-217.ngrok-free.app")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

//builder.WebHost.ConfigureKestrel(options =>
//{
//    options.ListenAnyIP(7265, listenOptions =>
//    {
//        listenOptions.UseHttps("certs/travel-api.pfx", "1234");
//    });
//});

builder.Services.AddSignalR();

builder.Services.AddControllers()
     .AddJsonOptions(options =>
     {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
     });

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHttpClient();

builder.Services.AddAuthorization();


builder.Services.Configure<ECPayConfiguration>(builder.Configuration.GetSection("ECPaySettings"));
builder.Services.AddScoped<ECPayService>();
builder.Services.AddHostedService<OrderExpirationService>();


var app = builder.Build();
// 啟用 Swagger
app.UseSwagger();
app.UseSwaggerUI();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var seeder = new TestDataSeeder(context);
        await seeder.SeedAsync();
    }
}

app.UseHttpsRedirection();
app.UseStaticFiles();

var mvcWwwroot = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "../TravelAgencyBackend/wwwroot"));
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(mvcWwwroot),
    RequestPath = "/uploads"
});

app.UseRouting();               

app.UseCors("AllowFrontend");  

app.UseAuthentication(); 
app.UseAuthorization();

app.MapControllers();
app.MapHub<ChatHub>("/chathub");


app.Run();