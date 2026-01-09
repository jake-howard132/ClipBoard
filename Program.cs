using Avalonia;
using Avalonia.Controls;
using Avalonia.Logging;
using Avalonia.WebView.Desktop;
using ClipBoard.Models;
using ClipBoard.Services;
using ClipBoard.ViewModels;
using ClipBoard.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using ReactiveUI.Avalonia;
using Splat;
using Splat.Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;


namespace ClipBoard
{
    internal sealed class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main(string[] args) => BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
        {
            if (Design.IsDesignMode)
            {
                return AppBuilder.Configure<App>()
                        .UsePlatformDetect()
                        .WithInterFont()
                        .LogToTrace()
                        .UseReactiveUI()
                        .UseDesktopWebView();
            }

            var services = ConfigureServices();

            return AppBuilder.Configure(() => new App(services))
                        .UsePlatformDetect()
                        .WithInterFont()
                        .LogToTrace()
                        .UseReactiveUI()
                        .UseDesktopWebView();

        }
        private static ServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            services
                .AddDbContext<Db>(options => options.UseSqlite("Data Source=ClipBoard.db"))
                .AddSingleton<ClipView>()
                .AddSingleton<ClipsViewModel>()
                .AddTransient<ClipGroup>()
                .AddTransient<Clip>()
                .AddScoped<WindowService>()
                .AddScoped<ClipboardService>()
                .AddScoped<ClipsRepository>()
                .AddScoped<ClipGroupsRepository>()
                .UseMicrosoftDependencyResolver();

            var serviceProvider = services.BuildServiceProvider();

            if (!Design.IsDesignMode)
            {
                // Run migrations
                using (var scope = serviceProvider.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<Db>();
                    db.Database.MigrateAsync();

                    if (!db.ClipGroups.Any(cg => cg.IsDefault))
                    {
                        db.ClipGroups.Add(
                            new ClipGroupRecord
                            {
                                Name = "Clipboard",
                                Description = "Clipboard History",
                                IsDefault = true
                            }
                        );

                        db.SaveChanges();
                    }
                }
            }

            // Splat / ReactiveUI setup
            var resolver = Locator.CurrentMutable;
            resolver.InitializeSplat();
            resolver.InitializeReactiveUI();
            resolver.RegisterConstant(serviceProvider, typeof(IServiceProvider));

            return serviceProvider;
        }
    }
}
