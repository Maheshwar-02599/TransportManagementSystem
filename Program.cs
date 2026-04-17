using Microsoft.EntityFrameworkCore;
using TransportationManagement.Data;
using TransportationManagement.Interfaces;
using TransportationManagement.Repositories;
using TransportationManagement.Services;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Session (used for login)
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddHttpContextAccessor();

// --- REPOSITORIES ---
builder.Services.AddScoped<IVehicleRepository, VehicleRepository>();
builder.Services.AddScoped<IDriverRepository, DriverRepository>();
builder.Services.AddScoped<ITripRepository, TripRepository>();
builder.Services.AddScoped<IMaintenanceRepository, MaintenanceRepository>();
builder.Services.AddScoped<IFuelRepository, FuelRepository>();
// Add this line:
builder.Services.AddScoped<AccountRepository>();

// --- SERVICES ---
builder.Services.AddScoped<VehicleService>();
builder.Services.AddScoped<DriverService>();
builder.Services.AddScoped<TripService>();
builder.Services.AddScoped<MaintenanceService>();
builder.Services.AddScoped<FuelService>();
// Add this line:
builder.Services.AddScoped<AccountService>();

// MVC
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Apply migrations automatically
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession(); // Important: Keep this before MapControllerRoute

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();