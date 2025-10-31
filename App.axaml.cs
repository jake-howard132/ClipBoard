using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using ClipBoard.Services;
using ClipBoard.ViewModels;
using ClipBoard.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using Splat;
using Splat.Microsoft.Extensions.DependencyInjection;
using System;

namespace ClipBoard
{
    public partial class App : Application
    {
        public static IServiceProvider Services { get; private set; } = null!;
        private Window? _ClipsView;

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

                _ClipsView = new ClipsView
                {
                    DataContext = Services.GetRequiredService<ClipsViewModel>()
                };
            }

            base.OnFrameworkInitializationCompleted();
        }

        private void NativeMenuItem_Click(object? sender, System.EventArgs e)
        {
            ClipsView sv = new ClipsView();
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