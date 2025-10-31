using ClipBoard.Models;
using ClipBoard.Services;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;

namespace ClipBoard.ViewModels
{
    public class ClipGroup : ReactiveObject
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public string? Description { get; set; }
        public virtual IList<Clip> Clips { get; set; } = new List<Clip>();
        public int SortOrder { get; set; }

        private bool _isEditing;
        public bool IsEditing
        {
            get => _isEditing;
            set => this.RaiseAndSetIfChanged(ref _isEditing, value);
        }
        public sealed class AddButtonMarker { } // sentinel type

        public ReactiveCommand<Unit, Unit> StartClipGroupNameEditCommand { get; }
        public ReactiveCommand<Unit, Unit> FinishClipGroupNameEditCommand { get; }

        public ClipGroup(Guid id, string name, string description, IList<Clip> clips, int sortOrder)
        {
            Id = id;
            Name = name;
            Description = description;
            Clips = clips;
            SortOrder = sortOrder;

            StartClipGroupNameEditCommand = ReactiveCommand.Create(() => { IsEditing = true; });

            FinishClipGroupNameEditCommand = ReactiveCommand.Create(() => { IsEditing = false; });
        }

        public static ClipGroup ToModel(ClipGroupRecord g) =>
            new(g.Id,
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
                Id = Id,
                Name = Name,
                Description = Description,
                Clips = Clips
                    .Select(c => c.ToRecord())
                    .ToList(),
                SortOrder = SortOrder
            };
    }
}