using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using ClipBoard.Services;
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
        public ObservableCollection<Clip> Clips { get; set; } = new();

        private ClipGroup? _selectedClipGroup;
        public ClipGroup? SelectedClipGroup
        {
            get => _selectedClipGroup;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedClipGroup, value);
                LoadClipsAsync().RunSynchronously();
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
        public ReactiveCommand<IEnumerable<Clip>, Unit> AddClipsCommand { get; }
        public ReactiveCommand<string, Unit> AddClipGroupCommand { get; }
        public ReactiveCommand<Clip, Unit> DeleteClipCommand { get; }
        public ReactiveCommand<Unit, Unit> ResequenceClipsCommand { get; }
        public ReactiveCommand<Unit, Unit> CloseCommand { get; }

        public ClipsViewModel(ClipsRepository clipsRepository, ClipGroupsRepository clipGroupsRepository)
        {
            _clipGroupsRepository = clipGroupsRepository;
            _clipsRepository = clipsRepository;

            LoadGroupsCommand = ReactiveCommand.CreateFromTask(LoadGroupsAsync);
            AddClipsCommand = ReactiveCommand.CreateFromTask<IEnumerable<Clip>>(AddClipsByGroupAsync);
            AddClipGroupCommand = ReactiveCommand.CreateFromTask<string>(clipGroupName => AddClipGroupAsync(clipGroupName, null));
            DeleteClipCommand = ReactiveCommand.CreateFromTask<Clip>(DeleteClipAsync);
            ResequenceClipsCommand = ReactiveCommand.CreateFromTask(ResequenceClips);
            CloseCommand = ReactiveCommand.Create(() => { });
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

            Clips.Clear();

            if (updatedGroup == null || updatedGroup.Clips == null)
                return;

            foreach (var clip in updatedGroup.Clips.OrderBy(c => c.SortOrder))
            {
                Clips.Add(
                    new Clip(
                        clip.Id,
                        clip.ClipGroupId,
                        updatedGroup.Name,
                        clip.Name,
                        clip.Description,
                        clip.Value,
                        clip.MimeType,
                        clip.CopyHotKey ?? "",
                        clip.PasteHotKey ?? "",
                        clip.SortOrder
                    ));
            }
        }

        private async Task AddClipsByGroupAsync(IEnumerable<Clip> newClips)
        {
            var newClipRecords = newClips.Select(c => c.ToRecord()).ToList();
            var clipGroups = newClips.GroupBy(c => c.ClipGroupId);

            foreach (var clipGroup in clipGroups)
            {
                var existing = Clips
                .Where(c => c.ClipGroupId == clipGroup.Key)
                .OrderBy(c => c.SortOrder)
                .ToList();

                int startingOrder = existing.Count;

                // Append the new clips with orders after existing ones
                int i = 0;
                foreach (var clip in clipGroup)
                {
                    clip.SortOrder = startingOrder + i;
                    Clips.Add(clip);
                    i++;
                }
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
        private async Task RenameClipGroupAsync(ClipGroup clipGroup, string newClipGroupName)
        {
            clipGroup.Name = newClipGroupName;
            var record = await _clipGroupsRepository.UpdateGroupAsync(clipGroup.ToRecord());
        }

        private async Task DeleteClipAsync(Clip c)
        {
            await _clipsRepository.DeleteClipAsync(c.Id);
            Clips.Remove(c);
            await ResequenceClips();
        }

        private async Task ResequenceClips()
        {
            for (int i = 0; i < Clips.Count; i++)
            {
                Clips[i].SortOrder = i;
            }
            await _clipsRepository.UpdateClipOrdersAsync(_selectedClipGroup.Id, Clips.Select(c => c.ToRecord()).ToList());
        }
    }
}
