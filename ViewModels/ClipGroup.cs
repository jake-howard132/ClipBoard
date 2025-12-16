using Avalonia.Collections;
using ClipBoard.Models;
using ClipBoard.Services;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ClipBoard.ViewModels
{
    public partial class ClipGroup : ReactiveObject
    {
        private readonly IServiceProvider _services;

        public int? Id { get; set; }

        private string _originalName { get; set; } = "";

        [Reactive] public string Name { get; set; }
        [Reactive] public string? Description { get; set; }
        [Reactive] public AvaloniaList<Clip> Clips { get; set; } = new();
        [Reactive] public int SortOrder { get; set; }
        [Reactive] public bool IsEditing { get; set; }

        public ReactiveCommand<Unit, Unit> AddClipCommand { get; }

        public ClipGroup(IServiceProvider services, int? id, string name, string description, IEnumerable<Clip> clips, int sortOrder, bool isEditing = false)
        {
            _services = services;
            this.Id = id;
            this.Name = name;
            this.Description = description;
            this.IsEditing = isEditing;
            this.Clips.AddRange(clips);
            this.SortOrder = sortOrder;

            AddClipCommand = ReactiveCommand.CreateFromTask(AddClipAsync);
        }

        public ClipGroup BeginRename()
        {
            this._originalName = Name;
            this.IsEditing = true;

            this.RaisePropertyChanged(string.Empty);
            return this;
        }

        public async Task<ClipGroup> ConfirmRename()
        {
            await _services
                .GetRequiredService<ClipGroupsRepository>()
                .UpdateClipGroupAsync(this.ToRecord());
            this.IsEditing = false;
            this.RaisePropertyChanged(string.Empty);

            return this;
        }

        public ClipGroup CancelRename()
        {
            this.Name = _originalName;
            this.IsEditing = false;

            this.RaisePropertyChanged(string.Empty);
            return this;
        }

        private async Task AddClipAsync()
        {
            var clipsRepository = _services.GetRequiredService<ClipsRepository>();

            var clip = new Clip(
                this._services,
                (int)this.Id!,
                this.Clips.Count
            );

            var record = await clipsRepository.AddClipAsync(clip.ToRecord());

            var newClip = Clip.ToModel(_services, record);
            this.Clips.Add(newClip);
            newClip.OpenClipCommand.Execute();
        }

        public async Task<bool> DeleteClipAsync(Clip clip)
        {
            if (clip.Id is null) return false;

            await _services
                .GetRequiredService<ClipsRepository>()
                .DeleteClipAsync((int)clip.Id);

            this.Clips.Remove(clip);
            return true;
        }

        public static ClipGroup ToModel(IServiceProvider services, ClipGroupRecord g)
        {
            return new ClipGroup(
                services,
                g.Id,
                g.Name,
                g.Description ?? "",
                g.Clips.Select(c => Clip.ToModel(services, c)),
                g.SortOrder
            );
        }

        public ClipGroupRecord ToRecord() =>
            new()
            {
                Id = this.Id,
                Name = this.Name,
                Description = this.Description,
                Clips = this.Clips.Select(c => c.ToRecord()),
                SortOrder = this.SortOrder
            };
    }
}