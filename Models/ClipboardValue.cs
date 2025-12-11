using Avalonia.Input;
using Avalonia.Platform.Storage;
using System.Collections.Generic;
using System.IO;

namespace ClipBoard.Models
{
    public class ClipboardContent
    {
        public string? Text { get; set; }
        public string? Rtf { get; set; }
        public MemoryStream? Image { get; set; }
    }
}
