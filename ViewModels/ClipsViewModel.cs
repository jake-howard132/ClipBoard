using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using ClipBoard.Services;
using DynamicData;
using ReactiveUI;
using ReactiveUI.Avalonia;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using System.Xml.Linq;
using Tmds.DBus.Protocol;

namespace ClipBoard.ViewModels
{
    public class ClipsViewModel : ReactiveObject
    {
        // Serivices
        private readonly ClipGroupsRepository _clipGroupsRepository;
        private readonly ClipsRepository _clipsRepository;

        // UI Properties
        public ObservableCollection<ClipGroup> ClipGroups { get; set; } = new();

        private ClipGroup? _selectedClipGroup;
        public ClipGroup? SelectedClipGroup
        {
            get => _selectedClipGroup;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedClipGroup, value);
                LoadClipsAsync();
            }
        }

        private bool _clipGroupNameEditing;
        public bool ClipGroupNameEditing
        {
            get => _clipGroupNameEditing;
            set => this.RaiseAndSetIfChanged(ref _clipGroupNameEditing, value);
        }

        // Commands
        public ReactiveCommand<Unit, Unit> LoadGroupsCommand { get; }
        public ReactiveCommand<string, Unit> AddClipGroupCommand { get; }
        public ReactiveCommand<IList<Clip>, Unit> AddClipsCommand { get; }
        public ReactiveCommand<ClipGroup, Unit> UpdateClipGroupCommand { get; }
        public ReactiveCommand<ClipGroup, Unit> CancelClipGroupUpdateCommand { get; }
        public ReactiveCommand<Clip, Unit> DeleteClipCommand { get; }
        public ReactiveCommand<Unit, Unit> ResequenceClipsCommand { get; }
        public ReactiveCommand<Unit, Unit> CloseCommand { get; }

        public ClipsViewModel(ClipsRepository clipsRepository, ClipGroupsRepository clipGroupsRepository)
        {
            _clipGroupsRepository = clipGroupsRepository;
            _clipsRepository = clipsRepository;

            LoadGroupsCommand = ReactiveCommand.CreateFromTask(LoadGroupsAsync);
            AddClipsCommand = ReactiveCommand.CreateFromTask<IList<Clip>>(AddClipsByGroupAsync);
            AddClipGroupCommand = ReactiveCommand.CreateFromTask<string>(clipGroupName => AddClipGroupAsync(clipGroupName, null));
            UpdateClipGroupCommand = ReactiveCommand.CreateFromTask<ClipGroup>(clipGroup => UpdateClipGroupAsync(clipGroup));
            CancelClipGroupUpdateCommand = ReactiveCommand.Create<ClipGroup>(clipGroup => clipGroup.CancelEdit());
            DeleteClipCommand = ReactiveCommand.CreateFromTask<Clip>(DeleteClipAsync);
            ResequenceClipsCommand = ReactiveCommand.CreateFromTask(ResequenceClips);
            CloseCommand = ReactiveCommand.Create(() => { });

            ClipGroups.Add(new ClipGroup(default(Guid), "sdfasdf", null, new List<Clip>(), 0));
            ClipGroups.Add(new ClipGroup(default(Guid), "sdfsdfsdfasdf", null, new List<Clip>(), 1));
        }

        private async Task LoadGroupsAsync()
        {
            ClipGroups.Clear();

            var groups = await _clipGroupsRepository.GetAllGroupsAsync();

            foreach (var group in groups)
                ClipGroups.Add(ClipGroup.ToModel(group));

            SelectedClipGroup ??= ClipGroups.FirstOrDefault();

            if (_selectedClipGroup != null)
                await LoadClipsAsync();
        }
        private async Task LoadClipsAsync()
        {
            var updatedGroup = await _clipsRepository.GetGroupByIdAsync(_selectedClipGroup.Id);
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

            //ResequenceClips();

        }
        private async Task AddClipGroupAsync(string clipGroupName, string? clipGroupDescription = null)
        {
            var clipGroup = new ClipGroup(
                default(Guid),
                clipGroupName,
                clipGroupDescription ?? "",
                new List<Clip>(),
                ClipGroups.Count);

            var record = await _clipGroupsRepository.AddClipGroupAsync(clipGroup.ToRecord());

            ClipGroups.Add(clipGroup);
        }
        private async Task UpdateClipGroupAsync(ClipGroup clipGroup)
        {
            var index = ClipGroups.ToList().FindIndex(g => g.Id == clipGroup.Id);
            if (index >= 0)
            {
                ClipGroups[index] = clipGroup;
            }

            clipGroup.ConfirmEdit();
            await _clipGroupsRepository.UpdateGroupAsync(clipGroup.ToRecord());
        }
        private async Task DeleteClipAsync(Clip c)
        {
            await _clipsRepository.DeleteClipAsync(c.Id);
            _selectedClipGroup.Clips.Remove(c);
            await ResequenceClips();
        }
        private async Task ResequenceClips()
        {
            for (int i = 0; i < _selectedClipGroup.Clips.Count; i++)
            {
                _selectedClipGroup.Clips[i].SortOrder = i;
            }
            await _clipsRepository.UpdateClipOrdersAsync(_selectedClipGroup.Id, _selectedClipGroup.Clips.Select(c => c.ToRecord()).ToList());
        }
    }
}
