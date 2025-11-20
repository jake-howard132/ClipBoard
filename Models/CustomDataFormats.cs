using Avalonia.Input;

namespace ClipBoard.Models
{
    public static class CustomDataFormats
    {
        public static readonly DataFormat Rtf = DataFormat.CreateStringPlatformFormat("application/rtf");
        public static readonly DataFormat Image = DataFormat.CreateBytesPlatformFormat("image/png");
    }
}
