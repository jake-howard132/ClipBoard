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
using System.Reactive.Linq;
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

        // Commands
        public ReactiveCommand<Unit, Unit> LoadGroupsCommand { get; }
        public ReactiveCommand<Unit, Unit> AddClipGroupCommand { get; }
        public ReactiveCommand<ClipGroup, Unit> DeleteClipGroupCommand { get; }
        public record RenameClipGroupParams(ClipGroup clipGroup, string newName);
        public ReactiveCommand<IList<Clip>, Unit> AddClipsCommand { get; }
        public ReactiveCommand<ClipGroup, Unit> CancelClipGroupUpdateCommand { get; }
        public ReactiveCommand<Clip, Unit> DeleteClipCommand { get; }
        public ReactiveCommand<ClipGroup, Unit> ResequenceClipsCommand { get; }
        public ReactiveCommand<Unit, Unit> CloseCommand { get; }

        public Interaction<string, bool> ConfirmDelete { get; } = new();


        public ClipsViewModel(ClipsRepository clipsRepository, ClipGroupsRepository clipGroupsRepository)
        {
            _clipGroupsRepository = clipGroupsRepository;
            _clipsRepository = clipsRepository;

            LoadGroupsCommand = ReactiveCommand.CreateFromTask(LoadGroupsAsync);
            AddClipsCommand = ReactiveCommand.CreateFromTask<IList<Clip>>(AddClipsByGroupAsync);
            AddClipGroupCommand = ReactiveCommand.CreateFromTask(AddClipGroupAsync);
            DeleteClipGroupCommand = ReactiveCommand.CreateFromTask<ClipGroup>(clipGroup => DeleteClipGroupAsync(clipGroup));
            CancelClipGroupUpdateCommand = ReactiveCommand.Create<ClipGroup>(clipGroup => clipGroup.CancelEdit());
            DeleteClipCommand = ReactiveCommand.CreateFromTask<Clip>(clip => DeleteClipAsync(clip));
            ResequenceClipsCommand = ReactiveCommand.CreateFromTask<ClipGroup>(clipGroup => ResequenceClipsAsync(clipGroup));
            CloseCommand = ReactiveCommand.Create(() => { });
        }

        private async Task LoadGroupsAsync()
        {
            ClipGroups.Clear();

            var groups = await _clipGroupsRepository.GetAllGroupsAsync();

            foreach (var group in groups)
                ClipGroups.Add(ClipGroup.ToModel(this._clipGroupsRepository, group));

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
                default(int),
                "New ClipGroup",
                "",
                new List<Clip>(),
                ClipGroups.Count,
                true);

            var record = await _clipGroupsRepository.AddClipGroupAsync(clipGroup.ToRecord());

            ClipGroups.Add(ClipGroup.ToModel(this._clipGroupsRepository, record));

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
