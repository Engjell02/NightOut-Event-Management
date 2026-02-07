using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Reservation_Management_App.Repository;
using Reservation_Management_App.Domain.Identity;
using Reservation_Management_App.Service.Interface;
using Reservation_Management_App.Service.Implementation;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<Reservation_Management_AppUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Generic Repository DI
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// Services DI
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<ILocationService, LocationService>();
builder.Services.AddScoped<IPerformerService, PerformerService>();
builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddScoped<IEventImportService, EventImportService>(); // CHANGED from Transient to Scoped

// External API
builder.Services.AddHttpClient<IExternalEventApiService, ExternalEventApiService>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

// Auto-import events from API on startup
using (var scope = app.Services.CreateScope())
{
    var importService = scope.ServiceProvider.GetRequiredService<IEventImportService>();
    await importService.ImportEventsFromApiAsync();
}

app.Run();
public partial class Program { }