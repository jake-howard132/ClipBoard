using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using AvaloniaWebView;
using ClipBoard.Services;
using ClipBoard.ViewModels;
using ClipBoard.Views;
using DryIoc.FastExpressionCompiler.LightExpression;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Net.Http.Headers;
using ReactiveUI;
using ScriptingBridge;
using Splat;
using Splat.Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace ClipBoard
{
    public partial class App : Application
    {
        public static IServiceProvider Services { get; private set; } = null!;
        private Window? _MainView;
        private Task? _serverTask;
        private int port = 2380;

        public App(IServiceProvider services)
        {
            Services = services;

            Services.GetRequiredService<Db>().Database.Migrate();
        }

        public override void Initialize() => AvaloniaXamlLoader.Load(this);

        public interface SaveRequest
        {
            public string value { get; }
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;

                _MainView = new MainView
                {
                    DataContext = Services.GetRequiredService<ClipsViewModel>(),
                    Height = 1000,
                    Width = 600
                };

                var app = CreateWebServer();

                _serverTask = Task.Run(async () =>
                {
                    await app.StartAsync(); // Starts server without blocking
                });
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
            if (_MainView == null)
                return;

            if (!_MainView.IsVisible)
            {
                _MainView.Activate();
                _MainView.Show();
            }
            else _MainView.Hide();
        }

        private WebApplication CreateWebServer()
        {
            var folder = Path.Combine(AppContext.BaseDirectory, "Assets", "clipview", "dist");

            var options = new WebApplicationOptions
            {
                WebRootPath = folder,
            };

            var builder = WebApplication.CreateBuilder(options);
            builder
                .WebHost
                .UseUrls("http://localhost:" + port);

            builder
                .Services
                .AddCors(options =>
                    {
                        options.AddPolicy(name: "any-origin",
                                builder =>
                                {
                                    builder
                                        .AllowAnyOrigin()
                                        .AllowAnyHeader()
                                        .AllowAnyMethod();
                                });
                    }
                )
                .AddControllers();
            
            var app = builder.Build();

            app
                .UseDefaultFiles()
                .UseStaticFiles(new StaticFileOptions
                {
                    FileProvider = new PhysicalFileProvider(folder),
                    RequestPath = "",
                    OnPrepareResponse = ctx =>
                    {
                        ctx.Context.Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate, max-age=0";
                        ctx.Context.Response.Headers["Pragma"] = "no-cache";
                        ctx.Context.Response.Headers["Expires"] = "0";
                    }
                })
                .UseRouting()
                .UseCors("any-origin");

            return app;
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