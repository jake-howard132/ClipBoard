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

        private string _name;
        public string Name
        {
            get => _name;
            set => this.RaiseAndSetIfChanged(ref _name, value);
        }
        private string OriginalName { get; set; } = "";

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

        public ClipGroup(IServiceProvider services, int? id, string name, string description, IEnumerable<Clip> clips, int sortOrder, bool isEditing = false)
        {
            _services = services;
            this.Id = id;
            this._name = name;
            this._description = description;
            this.IsEditing = isEditing;
            this.Clips = new AvaloniaList<Clip>(clips);
            this.SortOrder = sortOrder;
        }

        public ClipGroup BeginEdit()
        {
            this.OriginalName = _name;
            this.IsEditing = true;
            return this;
        }

        public async Task<ClipGroup> ConfirmEdit()
        {
            await _services
                .GetRequiredService<ClipGroupsRepository>()
                .UpdateGroupAsync(this.ToRecord());
            this.IsEditing = false;
            return this;
        }

        public ClipGroup CancelEdit()
        {
            this.Name = OriginalName;
            this.IsEditing = false;
            return this;
        }

        public static ClipGroup ToModel(IServiceProvider services, ClipGroupRecord g)
        {
            return new ClipGroup(
                services,
                g.Id,
                g.Name,
                g.Description ?? "",
                g.Clips
                    .Select(c => Clip.ToModel(services, g, c))
                    .OrderBy(c => c.SortOrder),
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