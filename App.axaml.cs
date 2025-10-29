using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using ClipBoard.Services;
using ClipBoard.ViewModels;
using ClipBoard.Views;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using Splat;
using Splat.Microsoft.Extensions.DependencyInjection;
using System;

namespace ClipBoard
{
    public partial class App : Application
    {
        private bool desktop;
        private TrayIcon? _trayIcon;
        private Window? _ClipsView;

        public override void Initialize() => AvaloniaXamlLoader.Load(this);

        public override void OnFrameworkInitializationCompleted()
        {
            var services = new ServiceCollection();

            // Database and repositories
            services.AddSingleton<Db>();
            services.AddSingleton<ClipsRepository>();
            services.AddSingleton<ClipGroupsRepository>();

            // ViewModels
            services.AddTransient<ClipsViewModel>();

            // Bridge Microsoft DI with Splat
            var resolver = Locator.CurrentMutable;
            services.UseMicrosoftDependencyResolver();
            var sp = services.BuildServiceProvider();

            // DI / Splat
            resolver.InitializeSplat();
            resolver.InitializeReactiveUI();
            resolver.RegisterConstant(sp, typeof(IServiceProvider));

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;

                _ClipsView = new ClipsView
                {
                    DataContext = new ClipsViewModel()
                };
            }

            base.OnFrameworkInitializationCompleted();
        }

        private void NativeMenuItem_Click(object? sender, System.EventArgs e)
        {
            ClipsView sv = new ClipsView();
            sv.DataContext = new ClipsViewModel();
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