using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using worbench.Data;
using worbench.Models;
using worbench.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Pobranie connection stringa z konfiguracji
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Konfiguracja EF Core z SQL Server
builder.Services.AddDbContext<WorkshopDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Rejestracja usług
builder.Services.AddControllersWithViews();

// Dodanie Identity i ról
builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<ApplicationRole>()
    .AddEntityFrameworkStores<WorkshopDbContext>();

// Rejestracja usług dla ICustomerService
builder.Services.AddScoped<ICustomerService, CustomerService>();

var app = builder.Build();



// Tworzenie ról, jeśli nie istnieją
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
    string[] roleNames = { "Admin", "Mechanik", "Klient" };

    foreach (var roleName in roleNames)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            var roleResult = await roleManager.CreateAsync(new ApplicationRole { Name = roleName });
            if (roleResult.Succeeded)
            {
                Console.WriteLine($"Role {roleName} created successfully.");
            }
            else
            {
                foreach (var error in roleResult.Errors)
                {
                    Console.WriteLine($"Error creating role {roleName}: {error.Description}");
                }
            }
        }
    }

    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    string adminEmail = "admin@admin2.pl";
    string adminPassword = "Admin123!"; // Ustaw silne hasło!

    if (await userManager.FindByEmailAsync(adminEmail) == null)
    {
        var adminUser = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true
        };
        var result = await userManager.CreateAsync(adminUser, adminPassword);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
            Console.WriteLine("Admin user created and assigned to 'Admin' role.");
        }
        else
        {
            foreach (var error in result.Errors)
            {
                Console.WriteLine(error.Description);
            }
        }
    }
}

// Konfiguracja pipeline'a HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Zmieniamy domyślną trasę na stronę logowania
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

// Opcjonalnie, jeśli chcesz zapewnić, że użytkownik nie jest zalogowany, zanim przekroczy inne strony:
app.Use(async (context, next) =>
{
    if (!context.User.Identity.IsAuthenticated)
    {
        context.Response.Redirect("/Account/Login");
    }
    else
    {
        await next.Invoke();
    }
});

app.MapRazorPages();

app.Run();

