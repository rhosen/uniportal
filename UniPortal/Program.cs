using Microsoft.AspNetCore.Identity;
using Serilog;
using UniPortal.Data.Seeders;
using UniPortal.Extensions;
using UniPortal.Middlewares;
using UniPortal.Services.Infrastructures;
using static UniPortal.Constants.AppConstant;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddRazorPages().AddRazorRuntimeCompilation();

        // Add Identity via extension
        builder.Services.AddCustomIdentity(builder.Configuration.GetConnectionString("DefaultConnection"));

        builder.Services.AddAuthentication("Cookies")
            .AddCookie("Cookies", options =>
            {
                options.LoginPath = AppRoutes.Login;
                options.LogoutPath = AppRoutes.Logout;
                options.ExpireTimeSpan = TimeSpan.FromHours(8);
            });


        builder.Services.AddRouting(options => options.LowercaseUrls = true);

        builder.Services.AddAppServices();

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .CreateLogger();

        var app = builder.Build();

        app.UseMiddleware<ExceptionHandlingMiddleware>();
        app.UseMiddleware<NotFoundHandlingMiddleware>();

        using (var scope = app.Services.CreateScope())
        {
            var initializer = scope.ServiceProvider.GetRequiredService<AppInitializer>();
            await initializer.InitializeAsync();
        }

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthorization();

        app.MapRazorPages();

        // Set login page as default route
        app.MapGet("/", context =>
        {
            context.Response.Redirect(AppRoutes.Login); // your login page route
            return Task.CompletedTask;
        });

        app.Run();
    }
}