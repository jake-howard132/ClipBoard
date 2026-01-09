using Avalonia.Controls;
using ClipBoard.ViewModels;
using ReactiveUI;
using ReactiveUI.Avalonia;
using System;
using System.Text.Json;
using System.Threading;

namespace ClipBoard.Views;

public partial class ClipView : ReactiveWindow<Clip>
{
    public ClipView()
    {
        InitializeComponent();

        var uri = new Uri("http://localhost:2380");

        this.WhenActivated(disposables =>
        {
            this.webView.NavigationCompleted += (s, e) =>
            {
                if (this.ViewModel is not Clip clip) return;

                this.webView.PostWebMessageAsString(clip.JsonValue is "" or null ? "{}" : clip.JsonValue, uri);

                this.webView.OpenDevToolsWindow();
            };

            this.DataContextChanged += (s, e) =>
            {
                if (this.ViewModel is not Clip clip) return;

                this.webView.PostWebMessageAsString(clip.JsonValue is "" or null ? "{}" : clip.JsonValue, uri);
            };

            this.webView.WebMessageReceived += (s, e) =>
            {
                if (this.ViewModel is not Clip clip) return;

                var message = JsonSerializer.Deserialize<JsonElement>(e.Message);

                message.TryGetProperty("json", out var json);
                message.TryGetProperty("text", out var text);

                clip.Value = text.ToString().Trim();
                clip.JsonValue = json.ToString();

                clip.UpdateClipCommand.Execute();
            };

            webView.Url = uri;
        });
    }

    private void Window_Closing(object? sender, WindowClosingEventArgs e)
    {
        this.Hide();
        e.Cancel = true;
    }
};