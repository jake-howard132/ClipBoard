using Avalonia.Collections;
using ClipBoard.Models;
using ClipBoard.Services;
using DynamicData;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace ClipBoard.ViewModels
{
    public partial class SettingsViewModel : ReactiveObject, IScreen
    {
        // Services
        private readonly SettingsRepository _settingsRepository;

        // UI Properties
        [Reactive] public AvaloniaList<string> SettingsGroups { get; set; } = new();

        //Routing
        public RoutingState Router => throw new NotImplementedException();

        // Commands
        public ReactiveCommand<Unit, Unit> LoadSettingsCommand { get; }

        public SettingsViewModel(IServiceProvider services)
        {
            _settingsRepository = services.GetRequiredService<SettingsRepository>();



            LoadSettingsCommand = ReactiveCommand.CreateFromTask(LoadSettingsAsync);
        }

        private async Task LoadSettingsAsync()
        {
            var settings = await _settingsRepository.GetSettingsAsync();

            if (settings == null) return;

            var settingsGroups = settings
                                    .Select(s => s.SettingsGroup)
                                    .Distinct()
                                    .ToList();

            SettingsGroups.AddRange<string>(settingsGroups);
        }

        private async Task<SettingsRecord> UpdateSettingAsync(SettingsRecord setting)
        {
            return await _settingsRepository.UpdateSettingAsync(setting);
        }
    }
}
