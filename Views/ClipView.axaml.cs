using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using AvaloniaWebView;
using ClipBoard.ViewModels;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Office2016.Drawing.Command;
using ReactiveUI;
using ReactiveUI.Avalonia;
using System;
using System.Drawing;
using System.IO;
using System.Security.Policy;
using WebKit;

namespace ClipBoard.Views;

public partial class ClipView : ReactiveWindow<Clip>
{
    public ClipView()
    {
        InitializeComponent();
        webView.Background = new SolidColorBrush(Colors.White);
        webView.Url= new Uri("http://localhost:5173/");
    }
};