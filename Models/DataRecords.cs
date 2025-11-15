using Avalonia.Collections;
using Avalonia.Input;
using ClipBoard.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace ClipBoard.Models
{
    public record ClipGroupRecord
    {
        [Required][DatabaseGenerated(DatabaseGeneratedOption.Identity)] public int? Id { get; set; }
        [Required] public string Name { get; set; } = "";
        public string? Description { get; set; }
        public IEnumerable<ClipRecord> Clips { get; set; } = new AvaloniaList<ClipRecord>();
        [Required] public int SortOrder { get; set; }
    };
    public record ClipRecord
    {
        [Required][DatabaseGenerated(DatabaseGeneratedOption.Identity)] public int? Id { get; set; }
        [Required] public int ClipGroupId { get; set; }
        [Required] public ClipGroupRecord? ClipGroup { get; set; }
        [Required] public string Name { get; set; } = "";
        public string? Description { get; set; }
        [Required] public string Value { get; set; } = "";
        [Required] public string JsonValue { get; set; } = "";
        [Required] public string MimeType { get; set; } = string.Empty;
        public string? CopyHotKey { get; set; }
        public string? PasteHotKey { get; set; }
        [Required] public int SortOrder { get; set; }
    }
}
