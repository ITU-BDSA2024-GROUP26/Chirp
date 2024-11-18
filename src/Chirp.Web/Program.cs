using System.Security.Claims;
using Chirp.Core.Entities;
using Chirp.Infrastructure.Repositories;
using Chirp.Infrastructure.Services;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Chirp.Razor;

public class Program
{
    public static async Task Main()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddRazorPages();

        // Load database connection via configuration
        // where we differentiate between production and development/staging/anything else 
        // Note that environment like this is set via the ASPNETCORE_ENVIRONMENT environment variable 
        // this is set globally to Production on our Azure server, so we don't need to worry about anything
        string? connectionString; 
        if(builder.Environment.IsDevelopment()) { // always use in memory for development now
            Console.WriteLine("Using in memory database");
            
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();
            
            builder.Services.AddDbContext<CheepDbContext>(options => {
                    options.ConfigureWarnings(warnings => 
                    warnings.Ignore(RelationalEventId.NonTransactionalMigrationOperationWarning));
                    options.UseSqlite(connection);}
                    );
            CheepDbContext.TestingSetup = true;
        } else {
            if(builder.Environment.IsEnvironment("Production")) {
                connectionString = builder.Configuration.GetConnectionString("ProductionConnection");
            } else {
                connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            }
            builder.Services.AddDbContext<CheepDbContext>(options => options.UseSqlite(connectionString));
        }

        builder.Services.AddDefaultIdentity<Author>(options =>
            {
                options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+ ";
            })
            .AddEntityFrameworkStores<CheepDbContext>();

        // add services via DI  
        builder.Services.AddScoped<ICheepRepository, CheepRepository>(); 
        builder.Services.AddScoped<ICheepService, CheepService>();
        // Taken from Helge
        builder.Services.AddAuthentication(options =>
        {
        /*options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = "GitHub";
        */
        })
        //.AddCookie()
        .AddGitHub(o =>
        {
        o.ClientId = builder.Configuration["authentication:github:clientId"] 
                     ?? Environment.GetEnvironmentVariable("GITHUBCLIENTID")
                     ?? throw new InvalidDataException("Github client id not found");
        o.ClientSecret = builder.Configuration["authentication:github:clientSecret"]
                         ?? Environment.GetEnvironmentVariable("GITHUBCLIENTSECRET")
                         ?? throw new InvalidDataException("Github client secret not found");
        o.CallbackPath = "/signin-github";
        
        o.Scope.Add("read:user");//access to the github-user's public profile information
        
        o.Scope.Add("user:email");//access to the github-user's primary email address
        o.Scope.Add("user:");
        
        o.ClaimActions.MapJsonKey(ClaimTypes.Name, "name");
        });
        
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
            if (app.Environment.IsDevelopment()) await context.Database.MigrateAsync();
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

