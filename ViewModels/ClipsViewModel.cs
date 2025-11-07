using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using ClipBoard.Services;
using ClipBoard.Views;
using DynamicData;
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
        private readonly WindowService _windowService;
        private readonly ClipGroupsRepository _clipGroupsRepository;
        private readonly ClipsRepository _clipsRepository;

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

        //Routing
        public RoutingState Router => throw new NotImplementedException();
        public ReactiveCommand<Clip?, Unit> OpenClipCommand { get; }


        // Commands
        public ReactiveCommand<Unit, Unit> LoadClipGroupsCommand { get; }
        public ReactiveCommand<Unit, Unit> AddClipGroupCommand { get; }
        public ReactiveCommand<ClipGroup, Unit> DeleteClipGroupCommand { get; }
        public ReactiveCommand<IList<Clip>, Unit> AddClipsCommand { get; }
        public ReactiveCommand<Clip, Unit> DeleteClipCommand { get; }
        public ReactiveCommand<ClipGroup, Unit> ResequenceClipsCommand { get; }
        public ReactiveCommand<Unit, Unit> CloseCommand { get; }


        //Interactions
        public Interaction<string, bool> ConfirmDelete { get; } = new();

        public ClipsViewModel(WindowService windowService, ClipsRepository clipsRepository, ClipGroupsRepository clipGroupsRepository)
        {
            _windowService = windowService;
            _clipGroupsRepository = clipGroupsRepository;
            _clipsRepository = clipsRepository;

            OpenClipCommand = ReactiveCommand.CreateFromTask<Clip?>(clip => OpenClipAsync(clip));

            LoadClipGroupsCommand = ReactiveCommand.CreateFromTask(LoadGroupsAsync);
            AddClipsCommand = ReactiveCommand.CreateFromTask<IList<Clip>>(AddClipsByGroupAsync);
            AddClipGroupCommand = ReactiveCommand.CreateFromTask(AddClipGroupAsync);
            DeleteClipGroupCommand = ReactiveCommand.CreateFromTask<ClipGroup>(clipGroup => DeleteClipGroupAsync(clipGroup));
            DeleteClipCommand = ReactiveCommand.CreateFromTask<Clip>(clip => DeleteClipAsync(clip));
            ResequenceClipsCommand = ReactiveCommand.CreateFromTask<ClipGroup>(clipGroup => ResequenceClipsAsync(clipGroup));
            CloseCommand = ReactiveCommand.Create(() => { });
        }

        private async Task OpenClipAsync(Clip? clip)
        {
            clip = clip ?? new Clip(
                this._clipsRepository,
                default(int),
                this._selectedClipGroup.Id,
                this._selectedClipGroup.Name,
                "New Clip",
                null,
                "",
                "",
                "",
                "",
                this._selectedClipGroup.Clips.Count);

            await this._windowService.OpenWindowAsync(clip);
        }

        private async Task LoadGroupsAsync()
        {
            ClipGroups.Clear();

            var groups = await _clipGroupsRepository.GetAllGroupsAsync();

            foreach (var group in groups)
                ClipGroups.Add(ClipGroup.ToModel(this._clipGroupsRepository, this._clipsRepository, group));

            SelectedClipGroup ??= ClipGroups.FirstOrDefault();
        }

        private async Task AddClipsByGroupAsync(IEnumerable<Clip> newClips)
        {
            var newClipRecords = newClips.Select(c => c.ToRecord()).ToList();
            var clipGroups = newClips.GroupBy(c => c.ClipGroupId);

            foreach (var group in clipGroups)
            {
                var clipGroup = ClipGroups.FirstOrDefault(g => g.Id == group.Key);
                clipGroup?.Clips.AddRange(group.ToList());
            }

            await _clipsRepository.AddClipsAsync(newClipRecords);
        }
        private async Task AddClipGroupAsync()
        {
            var clipGroup = new ClipGroup(
                this._clipGroupsRepository,
                this._clipsRepository, 
                default(int),
                "New ClipGroup",
                "",
                new List<Clip>(),
                ClipGroups.Count,
                true);

            var record = await _clipGroupsRepository.AddClipGroupAsync(clipGroup.ToRecord());

            ClipGroups.Add(ClipGroup.ToModel(this._clipGroupsRepository, this._clipsRepository, record));

            SelectedClipGroup = ClipGroups.Last();
            SelectedClipGroup.IsEditing = true;
        }
        private async Task DeleteClipGroupAsync(ClipGroup clipGroup)
        {
            var result = await ConfirmDelete.Handle("Are you sure?");
            if (!result) return;

            await _clipGroupsRepository.DeleteClipGroupAsync(clipGroup.ToRecord());
            ClipGroups.Remove(clipGroup);
            await ResequenceClipsAsync(clipGroup);
        }
        private async Task DeleteClipAsync(Clip clip)
        {
            var clipGroup = ClipGroups.FirstOrDefault(g => g.Id == clip.ClipGroupId);

            if (clipGroup == null) return;

            await _clipsRepository.DeleteClipAsync(clip.Id);
            clipGroup.Clips.Remove(clip);
            await ResequenceClipsAsync(clipGroup);
        }
        private async Task ResequenceClipsAsync (ClipGroup clipGroup)
        {
            for (int i = 0; i < clipGroup.Clips.Count; i++)
            {
                clipGroup.Clips[i].SortOrder = i;
            }
            await _clipsRepository.UpdateClipOrdersAsync(clipGroup.Id, clipGroup.Clips.Select(c => c.ToRecord()).ToList());
        }
    }
}
