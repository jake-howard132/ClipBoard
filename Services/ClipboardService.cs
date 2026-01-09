using Avalonia.Collections;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using ClipBoard.Models;
using ClipBoard.ViewModels;
using DryIoc.ImTools;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;


namespace ClipBoard.Services
{
    public class ClipboardService : IDisposable
    {
        private readonly IServiceProvider _services;
        private readonly ClipsRepository _clipsRepository;
        private readonly IClipboard _clipboard;
        private readonly DispatcherTimer _timer;

        private string? _lastText;
        private byte[]? _lastImageHash;

        public ClipboardService(IServiceProvider services, IClipboard clipboard)
        {
            _services = services;
            _clipsRepository = services.GetRequiredService<ClipsRepository>();
            _clipboard = clipboard;

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500)
            };

            _timer.Tick += async (_, _) => await AddClipboardContentAsync();
        }

        public void Start() => _timer.Start();
        public void Stop() => _timer.Stop();
        public void Dispose() => _timer.Stop();

        public async Task<Boolean> AddClipboardContentAsync()
        {
            try
            {
                using (var data = await _clipboard.TryGetDataAsync())
                {
                    if (data is null) return false;

                    switch (true)
                    {
                        case true when data.Formats.Contains(CustomDataFormats.Image):
                            var imageBytes = await data.TryGetValueAsync(CustomDataFormats.Image);

                            if (imageBytes.IsNullOrEmpty()) return false;

                            var hash = SHA256.HashData(imageBytes!);

                            if (_lastImageHash is not null && hash.SequenceEqual(_lastImageHash)) return false;

                            var imageClip = new Clip(_services, null, null, "image", imageBytes!.ToString()!);

                            await _clipsRepository.AddClipAsync(imageClip.ToRecord());

                            _lastImageHash = hash;

                            break;
                        case true when data.Formats.Contains(CustomDataFormats.Rtf):
                            var rtfString = await data.TryGetValueAsync(CustomDataFormats.Rtf);

                            if (rtfString is null || _lastText == rtfString) return false;

                            var rtfClip = new Clip(_services, null, null, "rtf", rtfString);
                            await _clipsRepository.AddClipAsync(rtfClip.ToRecord());

                            _lastText = rtfString;

                            break;
                        case true when data.Formats.Contains(DataFormat.Text):
                            var textString = await data.TryGetTextAsync();

                            if (textString is null || _lastText == textString) return false;

                            var textClip = new Clip(_services, null, null, "text", textString);
                            await _clipsRepository.AddClipAsync(textClip.ToRecord());

                            _lastText = textString;

                            break;
                    }

                    return true;
                }
            }
            catch { return false; }
        }
    }
}
