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
        [Required][DatabaseGenerated(DatabaseGeneratedOption.Identity)] public int Id { get; set; }
        [Required] public string Name { get; set; } = "";
        public string? Description { get; set; }
        public List<ClipRecord> Clips { get; set; } = new();
        public int SortOrder { get; set; }
    };
    public record ClipRecord
    {
        [Required][DatabaseGenerated(DatabaseGeneratedOption.Identity)] public int Id { get; init; }
        [Required] public int ClipGroupId { get; init; }
        [Required] public ClipGroupRecord ClipGroup { get; set; } = null!;
        [Required] public string Name { get; init; } = "";
        public string? Description { get; init; }
        [Required] public string Value { get; init; } = "";
        [Required] public string MimeType { get; set; } = string.Empty;
        public string? CopyHotKey { get; init; }
        public string? PasteHotKey { get; init; }
        public int SortOrder { get; set; }
    }
}
