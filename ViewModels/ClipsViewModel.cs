using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using ClipBoard.Models;
using ClipBoard.Services;
using ClipBoard.Views;
using DynamicData;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using ReactiveUI.Avalonia;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Tmds.DBus.Protocol;

namespace ClipBoard.ViewModels
{
    public class ClipsViewModel : ReactiveObject, IScreen
    {
        // Serivices
        private readonly IServiceProvider _services;

        // UI Properties

        private AvaloniaList<ClipGroup> _clipGroups = new();
        public AvaloniaList<ClipGroup> ClipGroups
        {
            get => _clipGroups;
            set => this.RaiseAndSetIfChanged(ref _clipGroups, value);
        }

        private ClipGroup? _selectedClipGroup;
        public ClipGroup? SelectedClipGroup
        {
            get => _selectedClipGroup;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedClipGroup, value);
            }
        }

        private bool _clipGroupNameEditing;
        public bool ClipGroupNameEditing
        {
            get => _clipGroupNameEditing;
            set => this.RaiseAndSetIfChanged(ref _clipGroupNameEditing, value);
        }

        private Clip? _openClip;
        public Clip? OpenClip
        {
            get => _openClip;
            set
            {
                this.RaiseAndSetIfChanged(ref _openClip, value);
            }
        }

        //Routing
        public RoutingState Router => throw new NotImplementedException();

        // Commands
        public ReactiveCommand<Unit, Unit> LoadClipGroupsCommand { get; }
        public ReactiveCommand<Unit, Unit> AddClipGroupCommand { get; }
        public ReactiveCommand<ClipGroup, Unit> DeleteClipGroupCommand { get; }
        public ReactiveCommand<Unit, Unit> AddClipCommand { get; }
        public ReactiveCommand<IAvaloniaList<Clip>, Unit> AddClipsCommand { get; }
        public ReactiveCommand<Clip, Unit> DeleteClipCommand { get; }
        public ReactiveCommand<ClipGroup, Unit> ResequenceClipsCommand { get; }
        public ReactiveCommand<Unit, Unit> CloseCommand { get; }


        //Interactions
        public Interaction<string, bool> ConfirmDelete { get; } = new();

        public ClipsViewModel(IServiceProvider services)
        {
            _services = services;

            LoadClipGroupsCommand = ReactiveCommand.CreateFromTask(LoadGroupsAsync);
            AddClipCommand = ReactiveCommand.CreateFromTask(AddClipAsync);
            AddClipsCommand = ReactiveCommand.CreateFromTask<IAvaloniaList<Clip>>(clips => AddClipsByGroupAsync(clips));
            AddClipGroupCommand = ReactiveCommand.CreateFromTask(AddClipGroupAsync);
            DeleteClipGroupCommand = ReactiveCommand.CreateFromTask<ClipGroup>(clipGroup => DeleteClipGroupAsync(clipGroup));
            DeleteClipCommand = ReactiveCommand.CreateFromTask<Clip>(clip => DeleteClipAsync(clip));
            ResequenceClipsCommand = ReactiveCommand.CreateFromTask<ClipGroup>(clipGroup => ResequenceClipsAsync(clipGroup));
            CloseCommand = ReactiveCommand.Create(() => { });
        }

        private async Task LoadGroupsAsync()
        {
            ClipGroups.Clear();

            var clipGroupsRepository = _services.GetRequiredService<ClipGroupsRepository>();

            var groups = await clipGroupsRepository.GetAllGroupsAsync();

            if (groups == null) return;

            ClipGroups.AddRange<ClipGroup>(groups);

            SelectedClipGroup = ClipGroups.FirstOrDefault();
        }
        private async Task AddClipAsync()
        {
            if (_selectedClipGroup == null) return;

            var clipsRepository = _services.GetRequiredService<ClipsRepository>();

            var newClip = new Clip(
                _services,
                null,
                _selectedClipGroup.Id,
                "New Clip",
                "",
                "",
                "",
                "",
                "",
                this._selectedClipGroup.Clips.Count
            );

            var clip = await clipsRepository.AddClipAsync(newClip);

            _selectedClipGroup.Clips.Add(clip);

            _openClip = clip;

            await clip.OpenClipCommand.Execute();
        }

        private async Task AddClipsByGroupAsync(IAvaloniaList<Clip> newClips)
        {
            var clipsRepository = _services.GetRequiredService<ClipsRepository>();
            var groupedClips = newClips.GroupBy(c => c.ClipGroupId);

            foreach (var group in groupedClips)
            {
                var clipGroup = ClipGroups.FirstOrDefault(g => g.Id == group.Key);
                clipGroup?.Clips.AddRange(group.ToList());
            }

            await clipsRepository.AddClipsAsync(newClips);
        }
        private async Task AddClipGroupAsync()
        {
            var clipGroupsRepository = _services.GetRequiredService<ClipGroupsRepository>();

            var clipGroup = new ClipGroup(
                _services,
                null,
                "New ClipGroup",
                "",
                new AvaloniaList<Clip>(),
                ClipGroups.Count,
                true);

            var record = await clipGroupsRepository.AddClipGroupAsync(clipGroup.ToRecord());

            ClipGroups.Add(ClipGroup.ToModel(_services, record));

            SelectedClipGroup = ClipGroups.Last();
            SelectedClipGroup.IsEditing = true;
        }
        private async Task DeleteClipGroupAsync(ClipGroup clipGroup)
        {
            var clipGroupsRepository = _services.GetRequiredService<ClipGroupsRepository>();

            var result = await ConfirmDelete.Handle("Are you sure?");
            if (!result) return;

            await clipGroupsRepository.DeleteClipGroupAsync(clipGroup.ToRecord());
            ClipGroups.Remove(clipGroup);
            await ResequenceClipsAsync(clipGroup);
        }
        private async Task DeleteClipAsync(Clip clip)
        {
            var clipsRepository = _services.GetRequiredService<ClipsRepository>();

            var clipGroup = ClipGroups.FirstOrDefault(g => g.Id == clip.ClipGroupId);

            if (clipGroup == null || clip.Id is null) return;

            await clipsRepository.DeleteClipAsync((int)clip.Id);
            clipGroup.Clips.Remove(clip);
            await ResequenceClipsAsync(clipGroup);
        }
        private async Task ResequenceClipsAsync (ClipGroup clipGroup)
        {
            var clipsRepository = _services.GetRequiredService<ClipsRepository>();

            for (int i = 0; i < clipGroup.Clips.Count; i++)
            {
                clipGroup.Clips[i].SortOrder = i;
            }
            await clipsRepository.UpdateClipOrdersAsync((int)clipGroup.Id!, clipGroup.Clips);
        }
    }
}
