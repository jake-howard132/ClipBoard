using Avalonia.Controls;
using ClipBoard.Models;
using ClipBoard.Services;
using ReactiveUI;
using ReactiveUI.Avalonia;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using Tmds.DBus.Protocol;

namespace ClipBoard.ViewModels
{
    public class ClipsViewModel : ReactiveObject

    {
        private readonly ClipsRepository _repository;]
        private readonly ClipGroup _group;

        public ObservableCollection<Clip> Clips { get; set; } = new();

        public ReactiveCommand<Unit, Unit> LoadCommand { get; }
        public ReactiveCommand<Unit, Unit> AddClipCommand { get; }
        public ReactiveCommand<Clip, Unit> DeleteClipCommand { get; }

        private ClipGroup _selectedTab = null!;
        public ClipGroup SelectedTab
        {
            get => _selectedTab;
            set => this.RaiseAndSetIfChanged(ref _selectedTab, value);
        }

        public ReactiveCommand<Unit, Unit> CloseCommand { get; }
        public ClipsViewModel(ClipsRepository repository)
        {
            _repository = repository;

            LoadCommand = ReactiveCommand.CreateFromTask(LoadAsync);
            AddClipCommand = ReactiveCommand.CreateFromTask(AddClipAsync);
            DeleteClipCommand = ReactiveCommand.CreateFromTask<Clip>(DeleteClipAsync);
            CloseCommand = ReactiveCommand.Create(() => { });
        }
        private async Task LoadAsync()
        {
            var updatedGroup = await _repository.GetGroupByIdAsync(_group.Id);

            Clips.Clear();

            foreach (var clip in updatedGroup.Clips.OrderBy(c => c.SortOrder))
            {
                Clips.Add(
                    new Clip(
                        clip.Id,
                        clip.ClipGroupId,
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

        private async Task AddClipAsync()
        {
            var clip = await _repository.AddClipAsync(_group.Id, "New Clip");

            Clips.Add(
                new Clip(
                    clip.ClipGroupId,
                    clip.Name,
                    clip.Description,
                    clip.Value,
                    clip.MimeType,
                    clip.CopyHotKey ?? "",
                    clip.PasteHotKey ?? "",
                    clip.SortOrder
                ));
            ResequenceDisplayOrder();
        }

        private async Task DeleteClipAsync(Clip c)
        {
            await _repository.DeleteClipAsync(c.Id);
            Clips.Remove(c);
            ResequenceDisplayOrder();
        }

        private async void ResequenceDisplayOrder()
        {
            for (int i = 0; i < Clips.Count; i++)
            {
                Clips[i].SortOrder = i;
            }
            await _repository.UpdateClipOrdersAsync(_group.Id, Clips.Select(c => c.ToRecord()).ToList());
        }
    }
}
