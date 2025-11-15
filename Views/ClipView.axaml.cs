using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using AvaloniaWebView;
using Cairo;
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
using System.Text.Json;
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

            this.webView.NavigationCompleted += (s, e) =>
            {
                if (this.ViewModel is not Clip clip) return;

                this.webView.PostWebMessageAsJson(clip.JsonValue, uri);
            };

            this.DataContextChanged += (s, e) =>
            {
                if (this.ViewModel is not Clip clip) return;
                this.webView.PostWebMessageAsJson(clip.JsonValue, uri);
            };

            this.webView.WebMessageReceived += (s, e) =>
            {
                if (this.ViewModel is not Clip clip) return;

                var message = JsonSerializer.Deserialize<JsonElement>(e.Message);

                message.TryGetProperty("json", out var json);
                message.TryGetProperty("text", out var text);

                clip.Value = text.ToString();
                clip.JsonValue = json.ToString();
            };

            webView.Url = uri;
        });

        
    }

    private void Window_Closing(object? sender, WindowClosingEventArgs e)
    {
        this.Hide();
        this.DataContext = null;
        e.Cancel = true;
    }
};