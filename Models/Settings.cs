using ClipBoard.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ClipBoard.Models
{
    public class Settings
    {
        //Application
        public bool RememberWindowPosition { get; set; } = true;
        public Tuple<int, int> DefaultWindowPosition { get; set; } = null!;
        public bool StartOnStartup { get; set; } = true;
        public string DatabasePath { get; set; } = "./ClipBoard.sqlite";
        public string Theme { get; set; } = "Light";
        public string OpenClipsHotKey { get; set; } = @"Ctrl + `";

        //ClipGroups
        public ClipGroup[] ClipGroups { get; set; } = Array.Empty<ClipGroup>();
        public int GroupHeaderFontSize { get; set; } = 16;
        public int GroupHeaderSpacing { get; set; } = 5;
        public bool AllowReorderClips { get; set; } = true;

        //Clips
        public bool ShowPasteIndicator { get; set; } = true;
        public int MaxClipSizeBytes { get; set; } = 0;

    }
}
