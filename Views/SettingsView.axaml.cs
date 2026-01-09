using Avalonia;
using Avalonia.Controls;
using ClipBoard.Models;
using ClipBoard.ViewModels;
using ReactiveUI;
using ReactiveUI.Avalonia;
using System;
using System.Text.Json;
using System.Threading;

namespace ClipBoard.Views;

public partial class SettingsView : ReactiveWindow<SettingsViewModel>
{
    public SettingsView()
    {
    }
};