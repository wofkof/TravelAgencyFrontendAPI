using Microsoft.EntityFrameworkCore;
using TravelAgencyFrontendAPI.Hubs;
using TravelAgency.Shared.Data;
using TravelAgencyFrontendAPI.Helpers;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));
// 加入你自己的寄信服務
builder.Services.AddScoped<EmailService>();

//新增 Swagger 設定
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();




// Add services to the container.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("https://localhost:3000", "https://localhost:7107", "https://localhost:7258", "https://192.168.1.122:3000")
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
builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

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
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "wwwroot", "Uploads")),
    RequestPath = "/Uploads"
});

app.UseRouting();               

app.UseCors("AllowFrontend");  

// app.UseAuthentication(); 
app.UseAuthorization();

app.MapControllers();
app.MapHub<ChatHub>("/chathub");

app.Run();