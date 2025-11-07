using Avalonia.Collections;
using AvRichTextBox;
using ClipBoard.Models;
using ClipBoard.Services;
using DocumentFormat.OpenXml.Bibliography;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.IO;

namespace ClipBoard.ViewModels
{
    public class Clip : ReactiveObject
    {
        private readonly ClipsRepository _clipsRepository;

        public Uri EditorURI { get; }

        public int Id { get; set; }
        public int ClipGroupId { get; set; }
        public string ClipGroupName { get; set; } = "";
        public string Name { get; set; } = "";
        public string? Description { get; set; }
        public virtual object Value { get; set; }
        public string MimeType { get; set; } = "";
        public string? CopyHotKey { get; set; }
        public string? PasteHotKey { get; set; }
        public int SortOrder { get; set; }


        public Clip(ClipsRepository clipsRepository, int id, int clipGroupId, string clipGroupName, string name, string? description, object value, string mimeType, string copyHotKey, string pasteHotKey, int sortOrder)
        {
            try
            {
                this.EditorURI = new Uri("avares://ClipBoard/Assets/Tiptap.html");
            }
            catch (Exception ex)
            {
                var test = ex;
            }

            _clipsRepository = clipsRepository;
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

        public static Clip ToModel(ClipsRepository clipsRepository, ClipRecord c) =>
            new(
                clipsRepository,
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
