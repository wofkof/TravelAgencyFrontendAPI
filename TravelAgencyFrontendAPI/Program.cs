using Microsoft.EntityFrameworkCore;
using TravelAgencyFrontendAPI.Data;
using TravelAgencyFrontendAPI.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
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

app.UseRouting();
app.UseCors("AllowFrontend");

app.UseStaticFiles();

app.UseAuthentication();

app.MapControllers();
app.MapHub<ChatHub>("/chathub");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
