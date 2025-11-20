using Avalonia.Collections;
using ClipBoard.Models;
using ClipBoard.Services;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ClipBoard.ViewModels
{
    public class ClipGroup : ReactiveObject
    {
        private readonly IServiceProvider _services;

        public int? Id { get; set; }

        private string _originalName { get; set; } = "";

        private string _name;
        public string Name
        {
            get => _name;
            set => this.RaiseAndSetIfChanged(ref _name, value);
        }

        private string _description;
        public string Description
        {
            get => _description;
            set => this.RaiseAndSetIfChanged(ref _description, value);
        }

        private IAvaloniaList<Clip> _clips;
        public IAvaloniaList<Clip> Clips
        {
            get => _clips;
            set => this.RaiseAndSetIfChanged(ref _clips, value);
        }
        public int SortOrder { get; set; }

        private bool _isEditing;
        public bool IsEditing
        {
            get => _isEditing;
            set => this.RaiseAndSetIfChanged(ref _isEditing, value);
        }
        public ReactiveCommand<Unit, Unit> AddClipCommand { get; }

        public ClipGroup(IServiceProvider services, int? id, string name, string description, IEnumerable<Clip> clips, int sortOrder, bool isEditing = false)
        {
            _services = services;
            this.Id = id;
            this._name = name;
            this._description = description;
            this._isEditing = isEditing;
            this._clips = new AvaloniaList<Clip>(clips);
            this.SortOrder = sortOrder;

            AddClipCommand = ReactiveCommand.CreateFromTask(AddClipAsync);
        }

        public ClipGroup BeginRename()
        {
            this._originalName = _name;
            this.IsEditing = true;
            return this;
        }

        public async Task<ClipGroup> ConfirmRename()
        {
            await _services
                .GetRequiredService<ClipGroupsRepository>()
                .UpdateClipGroupAsync(this.ToRecord());
            this.IsEditing = false;
            return this;
        }

        public ClipGroup CancelRename()
        {
            this.Name = _originalName;
            this.IsEditing = false;
            return this;
        }
        private async Task AddClipAsync()
        {
            var clipsRepository = _services.GetRequiredService<ClipsRepository>();

            var clip = new Clip(
                _services,
                null,
                this.Id,
                "New Clip",
                "",
                "",
                "",
                "",
                "",
                "",
                this.Clips.Count
            );

            var record = await clipsRepository.AddClipAsync(clip.ToRecord());

            var newClip = Clip.ToModel(_services, this.Id, record);
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
                g.Clips
                    .OrderBy(c => c.SortOrder)
                    .Select(c => Clip.ToModel(services, g.Id, c)),
                g.SortOrder
            );
        }

        public ClipGroupRecord ToRecord() =>
            new()
            {
                Id = this.Id,
                Name = this.Name,
                Description = this.Description,
                Clips = this.Clips
                    .OrderBy(c => c.SortOrder)
                    .Select(c => c.ToRecord()),
                SortOrder = this.SortOrder
            };
    }
}