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
        private Window? _ClipsView;
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

                _ClipsView = new ClipsView
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
            if (_ClipsView == null)
                return;

            if (!_ClipsView.IsVisible)
            {
                _ClipsView.Activate();
                _ClipsView.Show();
            }
            else _ClipsView.Hide();
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

            //_app?.MapControllers();
            
            var app = builder.Build();

            app
                .UseDefaultFiles()
                .UseStaticFiles(new StaticFileOptions
                {
                    FileProvider = new PhysicalFileProvider(folder),
                    RequestPath = ""
                })
                .UseRouting()
                .UseCors("any-origin");

            //app.MapPost("/saveclip", async (HttpRequest req) =>
            //{
            //    using (StreamReader reader = new StreamReader(req.Body, Encoding.UTF8))
            //    {
            //        var message = await reader.ReadToEndAsync();

            //        Dispatcher.UIThread.Post(() =>
            //        {
            //            var vm = Services.GetRequiredService<ClipsViewModel>();

            //            if (vm.SelectedClipGroup is null || vm.OpenClip is null) return;

                        
            //            var clip = vm.SelectedClipGroup.Clips.FirstOrDefault((c) => c.Id == vm.OpenClip.Id);

            //            if (clip is null) return;
            //            clip.Value = message;
            //        });

            //        return Results.Ok($"Data received: {message}");
            //    }
            //});

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