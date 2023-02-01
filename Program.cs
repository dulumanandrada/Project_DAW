using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OurApp.Data;
using OurApp.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();


//adaugam serviuciul AddRoles inainte de integrarea in baza de date
//nu mai avem IdentityUser si ApplicationUser -> modificam
builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();



var app = builder.Build();

// Pasul 5 - useri si roluri
//CreateScope ofera acces la instanta curenta a aplicatiei
//variabila scope are un Service Provider - folosit pentru a injecta dependentale in aplicatie
//dependente -> bd, cookie, sesiune, autentificare, etc.
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    SeedData.Initialize(services);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();

