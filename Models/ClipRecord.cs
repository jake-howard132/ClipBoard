using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClipBoard.Models
{
    public record ClipRecord
    {
        [Required][DatabaseGenerated(DatabaseGeneratedOption.Identity)] public int? Id { get; set; }
        [Required] public int? ClipGroupId { get; set; }
        [Required] public ClipGroupRecord? ClipGroup { get; set; }
        [Required] public string Name { get; set; } = "";
        public string? Description { get; set; }
        public string? Value { get; set; }
        public string? JsonValue { get; set; }
        [Required] public string MimeType { get; set; } = string.Empty;
        public string? CopyHotKey { get; set; }
        public string? PasteHotKey { get; set; }
        [Required] public int SortOrder { get; set; }
    }
}
