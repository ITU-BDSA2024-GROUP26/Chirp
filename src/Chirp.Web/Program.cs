using Chirp.Core.Entities;
using Chirp.Infrastructure.Repositories;
using Chirp.Infrastructure.Services;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace Chirp.Razor;

public class Program
{

    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddRazorPages();

        // Load database connection via configuration
        // where we differentiate between production and development/staging/anything else 
        // Note that enviroment like this is set via the ASPNETCORE_ENVIRONMENT enviroment variable 
        // this is set globally to Production on our Azure server, so we don't need to worry about anything
        string? connectionString; 
        if(builder.Environment.IsProduction()) {
            connectionString = builder.Configuration.GetConnectionString("ProductionConnection");
        } else {
            connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        }
        builder.Services.AddDbContext<CheepDbContext>(options => options.UseSqlite(connectionString));
        builder.Services.AddDefaultIdentity<Author>(options => options.SignIn.RequireConfirmedAccount = true)
            .AddEntityFrameworkStores<CheepDbContext>().AddEntityFrameworkStores<CheepDbContext>();

        // add services via DI  
        builder.Services.AddScoped<ICheepRepository, CheepRepository>(); 
        builder.Services.AddScoped<ICheepService, CheepService>();
        
        // Once you are sure everything works, you might want to increase this value to up to 1 or 2 years
        builder.Services.AddHsts(options => options.MaxAge = TimeSpan.FromHours(365));
        
        // inspired by, https://learn.microsoft.com/en-us/aspnet/core/security/cors?view=aspnetcore-8.0
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowChirp", policy =>
            {
                policy.WithOrigins("https://bdsagroup26chirprazor.azurewebsites.net"); //Only allow chirp
            });
        });
        
        var app = builder.Build();
        
        using (var scope = app.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<CheepDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Author>>();
            await DbInitializer.SeedDatabase(context, userManager);
        }

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();
        
        // Enable CORS policy
        app.UseCors("AllowChirp");
        
        
        // inspired by, https://learn.microsoft.com/en-us/aspnet/core/security/samesite?view=aspnetcore-9.0
        app.UseCookiePolicy(new CookiePolicyOptions
        {
            MinimumSameSitePolicy = SameSiteMode.Lax,     // Enforces "Lax" as the minimum SameSite policy
            HttpOnly = HttpOnlyPolicy.Always,             // Ensures cookies are only accessible over HTTP, not JavaScript
            //Secure = CookieSecurePolicy.Always          // Forces cookies to be sent only over HTTPS
        });

        app.UseAuthentication();
        app.UseAuthorization();
        
        app.MapRazorPages();

        await app.RunAsync();
    }
}

