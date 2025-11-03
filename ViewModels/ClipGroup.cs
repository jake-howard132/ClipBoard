using Avalonia.Collections;
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
        public string? OriginalName { get; private set; }
        public string? Description { get; set; }
        public virtual IAvaloniaList<Clip> Clips { get; set; } = new AvaloniaList<Clip>();
        public int SortOrder { get; set; }

        private bool _isEditing;
        public bool IsEditing
        {
            get => _isEditing;
            set => this.RaiseAndSetIfChanged(ref _isEditing, value);
        }
        public sealed class AddButtonMarker { } // sentinel type

        public ClipGroup(Guid id, string name, string description, IList<Clip> clips, int sortOrder)
        {
            Id = id;
            Name = name;
            Description = description;
            Clips = new AvaloniaList<Clip>(clips);
            SortOrder = sortOrder;
        }
        public void BeginEdit()
        {
            OriginalName = Name;
            IsEditing = true;
        }
        public void ConfirmEdit()
        {
            OriginalName = null;
            IsEditing = false;
        }
        public void CancelEdit()
        {
            if (OriginalName is not null)
                Name = OriginalName;

            OriginalName = null;
            IsEditing = false;
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