using Avalonia.Collections;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Media.Imaging;
using ClipBoard.Models;
using ClipBoard.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ClipBoard.Services
{
    public class ClipboardService
    {
        private readonly IClipboard _clipboard;

        public ClipboardService(IClipboard clipboard)
        {
            _clipboard = clipboard;
        }

        public async Task<Dictionary<DataFormat, object?>> GetContentAsync()
        {
            var content = new Dictionary<DataFormat, object?>();

            var data = await _clipboard.TryGetDataAsync();
            if (data == null)
                return content;

            using (data)
            {
                foreach (var fmt in data.Formats)
                {
                    switch (true)
                    {
                        case true when data.Contains(DataFormat.Text):
                            var text = await data.TryGetTextAsync();
                            content.Add(DataFormat.Text, text);
                            break;
                        case true when data.Contains(CustomDataFormats.Rtf):
                            var rtf = await data.TryGetValueAsync(CustomDataFormats.Rtf);
                            content.Add(CustomDataFormats.Rtf, rtf);
                            break;
                        case true when data.Contains(CustomDataFormats.Image):
                            var imageBytes = await data.TryGetValueAsync(CustomDataFormats.Image);
                            content.Add(CustomDataFormats.Image, imageBytes);
                            break;
                    }
                }
            }

            return content;
        }
    }
}
