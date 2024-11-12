using Chirp.Core.Entities;
using Chirp.Infrastructure.Repositories;
using Chirp.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
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
        // Note that environment like this is set via the ASPNETCORE_ENVIRONMENT environment variable 
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
                     ?? throw new InvalidDataException("Github client id not found");;
        o.ClientSecret = builder.Configuration["authentication:github:clientSecret"]
                         ?? Environment.GetEnvironmentVariable("GITHUBCLIENTSECRET")
                         ?? throw new InvalidDataException("Github client secret not found");
        o.CallbackPath = "/signin-github";
        
        o.Scope.Add("read:user");//access to the github-user's public profile information
        
        o.Scope.Add("user:email");//access to the github-user's primary email address

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

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapRazorPages();

        await app.RunAsync();
    }
}

