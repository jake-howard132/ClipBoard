using Avalonia.Collections;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Media.Imaging;
using ClipBoard.Models;
using ClipBoard.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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

        // -----------------------
        // SETTERS
        // -----------------------

        public Task SetTextAsync(string text)
        {
            var dt = new AsyncDataTransfer();
            dt.Add(DataFormat.Text, text);
            return _clipboard.SetDataAsync(dt);
        }

        public Task SetRtfAsync(string rtf)
        {
            var dt = new AsyncDataTransfer();
            dt.Add(CustomDataFormats.Rtf, rtf);
            return _clipboard.SetDataAsync(dt);
        }

        // Accept a stream (caller should ensure it's at position 0)
        public Task SetImageAsync(Stream imageStream, DataFormat format)
        {
            var dt = new AsyncDataTransfer();
            dt.Add(CustomDataFormats.Image, imageStream);
            return _clipboard.SetDataAsync(dt);
        }

    }
    public static class CustomDataFormats
    {
        public static readonly DataFormat Rtf = DataFormat.CreateStringPlatformFormat("application/rtf");
    }
}
