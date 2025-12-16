using Avalonia.Controls;
using ClipBoard.Models;
using ClipBoard.ViewModels;
using ClipBoard.Views;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using ReactiveUI.Avalonia;
using Splat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClipBoard.Services
{
    public class WindowService
    {
        private readonly IServiceProvider _services;
        public WindowService(IServiceProvider services)
        {
            _services = services;
        }

        public Task OpenWindowAsync<TViewModel>(TViewModel vm) where TViewModel : ReactiveObject
        {
            try
            {
                Window window = vm switch
                {
                    Clip => _services.GetRequiredService<ClipView>(),
                    SettingsViewModel => _services.GetRequiredService<SettingsView>(),
                    _ => throw new NotImplementedException($"No window registered for {typeof(TViewModel)}")
                };

                window.DataContext = vm;
                window.Show();
                window.Activate();
            }
            catch (Exception ex)
            {
                var test = ex;
            }

            return Task.CompletedTask;
        }
    }
}
