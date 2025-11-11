using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using AvaloniaWebView;
using ClipBoard.Services;
using ClipBoard.ViewModels;
using ClipBoard.Views;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using ReactiveUI;
using ScriptingBridge;
using Splat;
using Splat.Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;



namespace ClipBoard
{
    public partial class App : Application
    {
        public static IServiceProvider Services { get; private set; } = null!;
        private Window? _ClipsView;
        private WebApplication? _app;
        private Task? _serverTask;

        public App(IServiceProvider services)
        {
            Services = services;
        }

        public override void Initialize() => AvaloniaXamlLoader.Load(this);

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;

                var folder = Path.Combine(AppContext.BaseDirectory, "Assets", "clipview", "out");

                var options = new WebApplicationOptions 
                {
                    WebRootPath = folder,
                };

                var builder = WebApplication.CreateBuilder(options);
                builder
                    .WebHost
                    .UseUrls("http://localhost:2380");

                _app = builder.Build();

                _app.UseDefaultFiles(new DefaultFilesOptions
                    {
                        DefaultFileNames = new List<string> { "clipview.html" }
                    })
                    .UseStaticFiles(new StaticFileOptions
                    {
                        FileProvider = new PhysicalFileProvider(folder),
                        RequestPath = ""
                    });

                _serverTask = Task.Run(async () =>
                {
                    await _app.StartAsync(); // Starts server without blocking
                });

                _ClipsView = new ClipsView
                {
                    DataContext = Services.GetRequiredService<ClipsViewModel>(),
                    Height = 1000,
                    Width = 600
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
        public override void RegisterServices()
        {
            base.RegisterServices();
            AvaloniaWebViewBuilder.Initialize(default);
        }

        private void ToggleClipsView()
        {
            if (_ClipsView == null)
                return;

            if (!_ClipsView.IsVisible)
            {
                _ClipsView.Activate();
                _ClipsView.Show();
            }
            else _ClipsView.Hide();
        }

        private void View_Click(object? sender, System.EventArgs e)
        {
            ToggleClipsView();
        }
        private void Settings_Click(object? sender, System.EventArgs e)
        {
        }
        private void Exit_Click(object? sender, System.EventArgs e)
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.Shutdown();
            }
        }
    }
}