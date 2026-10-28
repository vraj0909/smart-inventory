using System;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using SmartInventory.Common;
using SmartInventory.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();

// Register HTTP State Management: Session and Memory Cache
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(45);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = ".SmartInventory.Session";
});

// Configure Database Connection with Pomelo MySQL & Resilient Fallback
var dbStatus = new DatabaseStatusService();
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=localhost;Port=3306;Database=smart_inventory_db;User=root;Password=;CharSet=utf8mb4;";

bool useMySql = true;

// Pre-test TCP connection to MySQL port 3306 so startup does not hang if MySQL is offline
try
{
    using var tcpClient = new System.Net.Sockets.TcpClient();
    var connectTask = tcpClient.ConnectAsync("127.0.0.1", 3306);
    if (!connectTask.Wait(TimeSpan.FromMilliseconds(800)))
    {
        useMySql = false;
        dbStatus.IsConnectedToMySql = false;
        dbStatus.ProviderName = "In-Memory Mode (MySQL port 3306 offline)";
        dbStatus.DiagnosticsMessage = "MySQL server is currently offline on port 3306. App is running in zero-friction demo mode. Start XAMPP/MySQL and refresh to connect to live MySQL.";
    }
}
catch
{
    useMySql = false;
    dbStatus.IsConnectedToMySql = false;
    dbStatus.ProviderName = "In-Memory Mode (MySQL offline)";
    dbStatus.DiagnosticsMessage = "MySQL port 3306 unreachable. Operating in demo mode.";
}

if (useMySql)
{
    try
    {
        var serverVersion = new MySqlServerVersion(new Version(8, 0, 36));
        builder.Services.AddDbContext<InventoryDbContext>(options =>
            options.UseMySql(connectionString, serverVersion, mySqlOptions =>
            {
                mySqlOptions.EnableRetryOnFailure(3);
            }));
        dbStatus.IsConnectedToMySql = true;
        dbStatus.ProviderName = "MySQL (Pomelo.EntityFrameworkCore.MySql)";
    }
    catch (Exception ex)
    {
        useMySql = false;
        builder.Services.AddDbContext<InventoryDbContext>(options =>
            options.UseInMemoryDatabase("SmartInventoryDb"));
        dbStatus.IsConnectedToMySql = false;
        dbStatus.ProviderName = "In-Memory Mode";
        dbStatus.DiagnosticsMessage = ex.Message;
    }
}
else
{
    builder.Services.AddDbContext<InventoryDbContext>(options =>
        options.UseInMemoryDatabase("SmartInventoryDb"));
}

builder.Services.AddSingleton(dbStatus);

var app = builder.Build();

// Ensure Database is Created & Seeded
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        var context = services.GetRequiredService<InventoryDbContext>();
        DbInitializer.Initialize(context);
        logger.LogInformation("Database initialized and seeded successfully using provider: {Provider}", dbStatus.ProviderName);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while seeding the database: {Message}", ex.Message);
    }
}

// Configure HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Enable Session state management
app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
