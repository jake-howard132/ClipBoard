using Avalonia;
using ClipBoard.Services;
using ClipBoard.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using ReactiveUI.Avalonia;
using Splat;
using Splat.Microsoft.Extensions.DependencyInjection;
using System;


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
            var services = ConfigureServices();

            return AppBuilder.Configure(() => new App(services))
                        .UsePlatformDetect()
                        .WithInterFont()
                        .LogToTrace()
                        .UseReactiveUI();
        }
        private static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            services
                .AddDbContext<Db>(options => options.UseSqlite("Data Source=ClipBoard.db"))
                .AddSingleton<ClipsViewModel>()
                .AddTransient<ClipGroup>()
                .AddScoped<ClipsRepository>()
                .AddScoped<ClipGroupsRepository>()
                .UseMicrosoftDependencyResolver();

            var sp = services.BuildServiceProvider();

            var resolver = Locator.CurrentMutable;
            resolver.InitializeSplat();
            resolver.InitializeReactiveUI();
            resolver.RegisterConstant(sp, typeof(IServiceProvider));
            return services.BuildServiceProvider();
        }
    }
}
