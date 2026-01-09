using Avalonia.Collections;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using ClipBoard.Models;
using ClipBoard.Services;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reactive;
using System.Threading.Tasks;
using System.Windows.Input;


namespace ClipBoard.ViewModels
{
    public partial class Clip : ReactiveObject
    {
        private readonly IServiceProvider? _services;

        public int? Id { get; set; }
        public int ClipGroupId { get; set; }
        public string? AppId { get; set; }
        public string? AppName { get; set; }
        public Bitmap? AppImage { get; set; }
        public DateTime Timestamp { get; set; }

        private string _originalName = "";

        [Reactive] public string Name { get; set; }
        [Reactive] public string? Description { get; set; }
        [Reactive] public string? Value { get; set; }
        [Reactive] public string? JsonValue { get; set; }
        [Reactive] public string ContentType { get; set; }
        [Reactive] public long sizeBytes { get; set; }
        [Reactive] public string? CopyHotKey { get; set; }
        [Reactive] public string? PasteHotKey { get; set; }
        [Reactive] public int SortOrder { get; set; }
        [Reactive] public bool IsEditing { get; set; }

        public ReactiveCommand<Unit, Unit> OpenClipCommand { get; }
        public ReactiveCommand<Unit, Clip> UpdateClipCommand { get; }
        public ReactiveCommand<Unit, Unit> DeleteClipCommand { get; }

        public Clip()
        {
            Name = "";
            ContentType = "";
            OpenClipCommand = ReactiveCommand.CreateFromTask(OpenClipAsync);
            UpdateClipCommand = ReactiveCommand.CreateFromTask<Clip>(UpdateClipAsync);
            DeleteClipCommand = ReactiveCommand.CreateFromTask(DeleteClipAsync);
        }

        public Clip(IServiceProvider services, string? appID, string? appName, string contentType, string value)
        {
            var jsonValue = contentType == "image" ? $"{{ \"type\":\"doc\",\"content\":[]}}" :
                            contentType == "html" ? $"{{ \"type\":\"doc\",\"content\":[]}}" :
                            contentType == "text" ? $"{{ \"type\":\"doc\",\"content\":[]}}" : null;

            _services = services;
            ClipGroupId = 0;
            AppId = appID;
            AppName = appName;
            Name = "New Clip";
            Value = value;
            JsonValue = jsonValue;
            ContentType = contentType;
            Timestamp = DateTime.UtcNow;

            OpenClipCommand = ReactiveCommand.CreateFromTask(OpenClipAsync);
            UpdateClipCommand = ReactiveCommand.CreateFromTask<Clip>(UpdateClipAsync);
            DeleteClipCommand = ReactiveCommand.CreateFromTask(DeleteClipAsync);
        }

        public Clip(IServiceProvider services, int clipGroupId)
        {
            _services = services;
            Id = null;
            ClipGroupId = clipGroupId;
            Name = "New Clip";
            Value = "";
            JsonValue = "{ \"type\":\"doc\",\"content\":[]}";
            ContentType = "text";
            Timestamp = DateTime.UtcNow;

            OpenClipCommand = ReactiveCommand.CreateFromTask(OpenClipAsync);
            UpdateClipCommand = ReactiveCommand.CreateFromTask<Clip>(UpdateClipAsync);
            DeleteClipCommand = ReactiveCommand.CreateFromTask(DeleteClipAsync);
        }

        public Clip(IServiceProvider services, int id, int clipGroupId, string? appId, string? appName, string name, string? description, string? value, string? jsonValue, string contentType, string copyHotKey, string pasteHotKey, int sortOrder, DateTime timestamp)
        {
            _services = services;
            Id = id;
            ClipGroupId = clipGroupId;
            AppId = appId;
            AppName = appName;
            Name = name;
            Description = description;
            Value = value;
            JsonValue = jsonValue;
            ContentType = contentType;
            CopyHotKey = copyHotKey;
            PasteHotKey = pasteHotKey;
            SortOrder = sortOrder;
            Timestamp = timestamp;

            OpenClipCommand = ReactiveCommand.CreateFromTask(OpenClipAsync);
            UpdateClipCommand = ReactiveCommand.CreateFromTask(UpdateClipAsync);
            DeleteClipCommand = ReactiveCommand.CreateFromTask(DeleteClipAsync);
        }

        public Clip BeginRename()
        {
            this._originalName = this.Name;
            this.IsEditing = true;
            this.RaisePropertyChanged(nameof(IsEditing));
            return this;
        }

        public Clip CancelRename()
        {
            this.Name = _originalName;
            this.IsEditing = false;
            this.RaisePropertyChanged(nameof(IsEditing));
            return this;
        }

        public async Task<Clip> ConfirmRename()
        {
            var clipRecord = this.ToRecord();

            await _services
                    .GetRequiredService<ClipsRepository>()
                    .UpdateClipAsync(clipRecord);

            this.IsEditing = false;
            this.RaisePropertyChanged(string.Empty);

            return this;
        }

        private async Task OpenClipAsync()
        {
            var windowService = _services.GetRequiredService<WindowService>();

            await windowService.OpenWindowAsync(this);
        }

        public async Task<Clip> UpdateClipAsync()
        {
            var clipRecord = this.ToRecord();

            await _services
                    .GetRequiredService<ClipsRepository>()
                    .UpdateClipAsync(clipRecord);

            this.IsEditing = false;
            this.RaisePropertyChanged(string.Empty);

            return this;
        }

        private async Task DeleteClipAsync()
        {
            await _services.GetRequiredService<ClipGroup>().DeleteClipAsync(this);
        }

        public static Clip ToModel(IServiceProvider services, ClipRecord c) =>
            new(
                services,
                (int)c.Id!,
                c.ClipGroupId,
                c.AppId,
                c.AppName,
                c.Name,
                c.Description,
                c.Value,
                c.JsonValue,
                c.ContentType,
                c.CopyHotKey ?? "",
                c.PasteHotKey ?? "",
                c.SortOrder,
                DateTimeOffset
                    .FromUnixTimeSeconds(c.Timestamp)
                    .DateTime
            );

        public ClipRecord ToRecord() =>
            new()
            {
                Id = Id,
                ClipGroupId = ClipGroupId,
                AppId = AppId,
                AppName = AppName,
                Name = Name,
                Description = Description,
                Value = Value,
                JsonValue = JsonValue,
                ContentType = ContentType,
                CopyHotKey = CopyHotKey,
                PasteHotKey = PasteHotKey,
                SortOrder = SortOrder,
                Timestamp = new DateTimeOffset(Timestamp).ToUnixTimeSeconds()
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
