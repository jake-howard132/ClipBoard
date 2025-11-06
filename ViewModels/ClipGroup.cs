using Avalonia.Collections;
using ClipBoard.Models;
using ClipBoard.Services;
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
        private readonly ClipGroupsRepository _clipGroupsRepository;

        public int Id { get; set; }

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
        public virtual IAvaloniaList<Clip> Clips { get; set; } = new AvaloniaList<Clip>();
        public int SortOrder { get; set; }

        private bool _isEditing;
        public bool IsEditing
        {
            get => _isEditing;
            set => this.RaiseAndSetIfChanged(ref _isEditing, value);
        }

        public ClipGroup(ClipGroupsRepository clipGroupsRepository, int id, string name, string description, IEnumerable<Clip> clips, int sortOrder, bool isEditing = false)
        {
            _clipGroupsRepository = clipGroupsRepository;
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
            await _clipGroupsRepository.UpdateGroupAsync(this.ToRecord());
            this.IsEditing = false;
            return this;
        }
        public ClipGroup CancelEdit()
        {
            this.Name = OriginalName;
            this.IsEditing = false;
            return this;
        }
        public static ClipGroup ToModel(ClipGroupsRepository clipGroupsRepository, ClipGroupRecord g) =>
            new(
                clipGroupsRepository,
                g.Id,
                g.Name,
                g.Description ?? "",
                g.Clips
                    .OrderBy(c => c.SortOrder)
                    .Select(c => Clip.ToModel(c))
                    .ToList(),
                g.SortOrder
            );

        public ClipGroupRecord ToRecord() =>
            new()
            {
                Id = this.Id,
                Name = this.Name,
                Description = this.Description,
                Clips = this.Clips
                    .Select(c => c.ToRecord())
                    .ToList(),
                SortOrder = this.SortOrder
            };
    }
}