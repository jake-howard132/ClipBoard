using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using AvaloniaWebView;
using ClipBoard.ViewModels;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Office2016.Drawing.Command;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using ReactiveUI;
using ReactiveUI.Avalonia;
using System;
using System.Drawing;
using System.IO;
using System.Security.Policy;
using System.Xml;
using WebKit;


namespace ClipBoard.Views;

public partial class ClipView : ReactiveWindow<Clip>
{
    public ClipView()
    {
        var uri = new Uri("http://localhost:2380");

        this.WhenActivated(disposables =>
        {
            InitializeComponent();

            webView.NavigationCompleted += (s, e) =>
            {
                if (this.ViewModel is not Clip vm) return;

                //var escaped = System.Text.Json.JsonSerializer.Serialize(strValue);

                LoadWebviewContent(vm.Value);
            };

            webView.Url = uri;
        });
    }

    private void LoadWebviewContent(string content)
    {
        webView.PostWebMessageAsJson("{ type: 'SetEditorContent', payload: " + content ?? "<p>Nothing to see here!</p>" + " }", new Uri("http://localhost:2380"));
    }

    private void Window_Closing(object? sender, WindowClosingEventArgs e)
    {
        this.Hide();
        e.Cancel = true;
    }
};