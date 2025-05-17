using Microsoft.EntityFrameworkCore;
using TravelAgencyFrontendAPI.Hubs;
using TravelAgency.Shared.Data;
using TravelAgencyFrontendAPI.Helpers;

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
        policy.WithOrigins("http://localhost:3000", "https://localhost:7107" , "https://localhost:7258")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

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

app.UseRouting();               

app.UseCors("AllowFrontend");  

// app.UseAuthentication(); 
app.UseAuthorization();

app.MapControllers();
app.MapHub<ChatHub>("/chathub");

app.Run();