using Avalonia.Input;
using ClipBoard.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace ClipBoard.Models
{
    public record ClipGroupRecord
    {
        [Required] public int Id { get; init; }
        [Required] public string Name { get; init; } = "";
        public string? Description { get; init; }
        public List<ClipRecord> Clips { get; init; } = new();
        public int SortOrder { get; init; }
    };
    public record ClipRecord
    {
        [Required] public int Id { get; init; }
        [Required] public int ClipGroupId { get; init; }
        [Required] public ClipGroupRecord ClipGroup { get; set; } = null!;
        [Required] public string Name { get; init; } = "";
        public string? Description { get; init; }
        [Required] public byte[] Value { get; init; } = Array.Empty<byte>();
        [Required] public string MimeType { get; set; } = string.Empty;
        public string? CopyHotKey { get; init; }
        public string? PasteHotKey { get; init; }
        public int SortOrder { get; init; }

        
    }
}
