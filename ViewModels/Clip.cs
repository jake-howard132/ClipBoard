using ClipBoard.Models;
using ReactiveUI;
using System;
using System.IO;

namespace ClipBoard.ViewModels
{
    public class Clip : ReactiveObject
    {
        public Guid Id { get; set; }
        public Guid ClipGroupId { get; set; }
        public string ClipGroupName { get; set; } = "";
        public string Name { get; set; } = "";
        public string? Description { get; set; }
        public object Value { get; set; } = Array.Empty<byte>();
        public string MimeType { get; set; } = "";
        public string? CopyHotKey { get; set; }
        public string? PasteHotKey { get; set; }
        public int SortOrder { get; set; }


        public Clip(Guid id, Guid clipGroupId, string clipGroupName, string name, string? description, object value, string mimeType, string copyHotKey, string pasteHotKey, int sortOrder)
        {
            Id = id;
            ClipGroupId = clipGroupId;
            ClipGroupName = clipGroupName;
            Name = name;
            Description = description;
            Value = value;
            MimeType = mimeType;
            CopyHotKey = copyHotKey;
            PasteHotKey = pasteHotKey;
            SortOrder = sortOrder;
        }

        public static Clip ToModel(ClipRecord c) =>
            new(
                c.Id,
                c.ClipGroupId,
                c.ClipGroup.Name,
                c.Name,
                c.Description,
                DecodeValue(c.MimeType, c.Value),
                c.MimeType,
                c.CopyHotKey ?? "",
                c.PasteHotKey ?? "",
                c.SortOrder
            );

        public ClipRecord ToRecord() =>
            new()
            {
                Id = Id,
                ClipGroupId = ClipGroupId,
                Description = Description,
                Value = EncodeValue(MimeType, Value),
                CopyHotKey = CopyHotKey,
                PasteHotKey = PasteHotKey,
                MimeType = MimeType,
                SortOrder = SortOrder
            };


        private static object DecodeValue(string mimeType, byte[] bytes)
        {
            return mimeType switch
            {
                "text/plain" => System.Text.Encoding.UTF8.GetString(bytes),
                "image/png" => new Avalonia.Media.Imaging.Bitmap(new MemoryStream(bytes)),
                _ => bytes
            };
        }
        private static byte[] EncodeValue(string mimeType, object value)
        {
            return mimeType switch
            {
                "text/plain" => System.Text.Encoding.UTF8.GetBytes((string)value),
                "image/png" => EncodeBitmapToBytes((Avalonia.Media.Imaging.Bitmap)value),
                _ => (byte[])value
            };
        }
        private static byte[] EncodeBitmapToBytes(Avalonia.Media.Imaging.Bitmap bmp)
        {
            using var ms = new MemoryStream();
            bmp.Save(ms);
            return ms.ToArray();
        }
    }
}
