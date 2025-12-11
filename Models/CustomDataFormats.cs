using Avalonia.Input;

namespace ClipBoard.Models
{
    public static class CustomDataFormats
    {
        public static readonly DataFormat<string> Rtf = DataFormat.CreateStringPlatformFormat("application/rtf");
        public static readonly DataFormat<byte[]> Image = DataFormat.CreateBytesPlatformFormat("image/png");
    }
}
