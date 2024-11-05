using Chirp.Core.Entities;
using Chirp.Infrastructure.Repositories;
using Chirp.Infrastructure.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Chirp.Razor;

public class Program
{

    public static async Task Main()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddRazorPages();

        // Load database connection via configuration
        // where we differentiate between production and development/staging/anything else 
        // Note that enviroment like this is set via the ASPNETCORE_ENVIRONMENT enviroment variable 
        // this is set globally to Production on our Azure server, so we don't need to worry about anything
        string? connectionString; 
        if(builder.Environment.IsDevelopment()) { // always use in memory for development now
            Console.WriteLine("Using in memory database");

            var _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            builder.Services.AddDbContext<CheepDBContext>(options => 
                    options.UseSqlite(_connection)
                    );
            CheepDBContext.testingSetup = true; 
            
        } else {

            if(builder.Environment.IsEnvironment("Production")) {
                connectionString = builder.Configuration.GetConnectionString("ProductionConnection");
            } else {
                connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            }
            builder.Services.AddDbContext<CheepDBContext>(options => options.UseSqlite(connectionString));
        
        }

        builder.Services.AddDefaultIdentity<ChirpUser>(options => options.SignIn.RequireConfirmedAccount = true)
            .AddEntityFrameworkStores<CheepDBContext>();

        // add services via DI  
        builder.Services.AddScoped<ICheepRepository, CheepRepository>(); 
        builder.Services.AddScoped<ICheepService, CheepService>();
        
        var app = builder.Build();

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

