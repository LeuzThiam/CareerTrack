using CareerTrack.Data;
using CareerTrack.Models;
using CareerTrack.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        // LocalDB peut mettre un instant à répondre sur la toute première connexion
        // après une période d'inactivité (démarrage à froid) ; sans nouvelle tentative,
        // EF Core abandonne immédiatement au lieu de laisser LocalDB finir de démarrer.
        sqlOptions => sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorNumbersToAdd: null)));

builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
    {
        options.Password.RequiredLength = 8;
        options.Password.RequireNonAlphanumeric = false;
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddSingleton<IFileStorageService, LocalFileStorageService>();
builder.Services.AddHealthChecks().AddDbContextCheck<ApplicationDbContext>();
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapStaticAssets();
app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapHealthChecks("/health");

// Hors développement, les migrations doivent normalement être revues puis appliquées
// manuellement (section 56 du cahier des charges). Le conteneur Docker n'embarque pas
// les outils EF Core ; APPLY_MIGRATIONS_ON_STARTUP=true offre un chemin explicite pour
// les environnements de démonstration où ce contrôle manuel n'est pas nécessaire.
if (app.Configuration.GetValue<bool>("APPLY_MIGRATIONS_ON_STARTUP"))
{
    using IServiceScope migrationScope = app.Services.CreateScope();
    await migrationScope.ServiceProvider
        .GetRequiredService<ApplicationDbContext>()
        .Database.MigrateAsync();
}

await AppRoleSeeder.SeedAsync(app.Services);

app.Run();
