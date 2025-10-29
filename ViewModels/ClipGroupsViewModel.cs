using Avalonia.Controls;
using ClipBoard.Models;
using ReactiveUI;
using ReactiveUI.Avalonia;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using Tmds.DBus.Protocol;

namespace ClipBoard.ViewModels
{
    public class ClipGroupsViewModel : ReactiveObject

    {
        public string Greeting { get; } = "Welcome to Avalonia!";

        public ObservableCollection<ClipGroup> ClipGroups { get; set; } = new();

        private ClipGroup _selectedTab;
        public ClipGroup SelectedTab
        {
            get => _selectedTab;
            set => this.RaiseAndSetIfChanged(ref _selectedTab, value);
        }

        public ReactiveCommand<Unit, Unit> CloseCommand { get; }
        public ClipGroupsViewModel()
        {
            CloseCommand = ReactiveCommand.Create(() => { });
            _selectedTab = ClipGroups.First();
        }
    }
}
