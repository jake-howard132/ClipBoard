using Avalonia.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
}
