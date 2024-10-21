using Chirp.Infrastructure.Repositories;
using Chirp.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace Chirp.Razor;

public class Program
{

    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddRazorPages();

        // Load database connection via configuration
        string? connectionString = builder.Configuration.GetConnectionString("MyTestDb") ?? "DefaultConnection";
        builder.Services.AddDbContext<CheepDBContext>(options =>
        options.UseInMemoryDatabase(builder.Configuration.GetConnectionString("MyTestDb")!)
        );
        //options.UseSqlite(connectionString));

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

        app.MapRazorPages();

        app.Run();
    }
}

