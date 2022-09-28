using ASP_Webapp.Data;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// Set up logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Add services and db context to the container.
var connectionString = builder.Configuration.GetConnectionString("LocalDatabase");
builder.Services.AddDbContext<CompanyContext>(options => {
    options.UseSqlite(connectionString);
    //options.EnableSensitiveDataLogging(true);
});

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}


app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
