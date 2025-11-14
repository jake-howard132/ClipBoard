using Avalonia.Collections;
using AvRichTextBox;
using ClipBoard.Models;
using ClipBoard.Services;
using ClipBoard.Views;
using DocumentFormat.OpenXml.Bibliography;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive;
using System.Threading.Tasks;


namespace ClipBoard.ViewModels
{
    public class Clip : ReactiveObject
    {
        private readonly IServiceProvider _services;

        public int? Id { get; set; }
        public int? ClipGroupId { get; set; }

        private string _name = "";
        public string Name
        {
            get => _name;
            set => this.RaiseAndSetIfChanged(ref _name, value);
        }

        private string _description = "";
        public string Description
        {
            get => _description ?? "";
            set => this.RaiseAndSetIfChanged(ref _description, value);
        }

        private string _value = "";
        public string Value
        {
            get => _value;
            set => this.RaiseAndSetIfChanged(ref _value, value);
        }
        public string MimeType { get; set; } = "";
        public string? CopyHotKey { get; set; }
        public string? PasteHotKey { get; set; }
        public int SortOrder { get; set; }

        public ReactiveCommand<Unit, Unit> OpenClipCommand { get; }

        public Clip(IServiceProvider services, int? id, int? clipGroupId, string name, string? description, string value, string mimeType, string copyHotKey, string pasteHotKey, int sortOrder)
        {
            _services = services;
            Id = id;
            ClipGroupId = clipGroupId;
            Name = name;
            Description = description ?? "";
            Value = value;
            MimeType = mimeType;
            CopyHotKey = copyHotKey;
            PasteHotKey = pasteHotKey;
            SortOrder = sortOrder;

            OpenClipCommand = ReactiveCommand.CreateFromTask(OpenClipAsync);
        }
        private async Task OpenClipAsync()
        {
            var windowService = _services.GetRequiredService<WindowService>();

            await windowService.OpenWindowAsync(this);
        }

        public static Clip ToModel(IServiceProvider services, ClipGroupRecord g, ClipRecord c) =>
            new(
                services,
                c.Id,
                g.Id,
                c.Name,
                c.Description,
                c.Value,
                c.MimeType,
                c.CopyHotKey ?? "",
                c.PasteHotKey ?? "",
                c.SortOrder
            );

        public ClipRecord ToRecord() =>
            new()
            {
                Id = Id,
                Description = Description,
                Value = Value,
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
