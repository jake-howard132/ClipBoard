using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using AvaloniaWebView;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using WebViewCore.Events;

namespace ClipBoard.Views.Behaviors
{
    public static class WebViewBehaviors
    {
        public static readonly AttachedProperty<ICommand?> LoadClipContentProperty = AvaloniaProperty.RegisterAttached<Control, ICommand?>("LoadClipContent", typeof(WebViewBehaviors));

        static WebViewBehaviors()
        {
            LoadClipContentProperty.Changed.AddClassHandler<WebView>((control, e) =>
            {
                control.NavigationCompleted -= LoadClipContent;
                control.NavigationCompleted += LoadClipContent;
            });
        }

        private static void LoadClipContent(object? sender, WebViewUrlLoadedEventArg e)
        {
            if (sender is not WebView control) return;

            control.WebMessageReceived -= SaveClipContent;
            control.WebMessageReceived += SaveClipContent;

            var command = GetLoadClipContent(control);

            if (command?.CanExecute(command) == true)
                command.Execute(command);
        }

        private static void SaveClipContent(object? sender, WebViewMessageReceivedEventArgs e)
        {
            
            //var message = System.Text.Json.JsonSerializer.(e.MessageAsJson);
        }


        public static void SetLoadClipContent(AvaloniaObject element, ICommand? value) => element.SetValue(LoadClipContentProperty, value);
        public static ICommand? GetLoadClipContent(AvaloniaObject element) => element.GetValue(LoadClipContentProperty);
    }
}
