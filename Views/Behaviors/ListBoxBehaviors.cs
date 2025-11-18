using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using System.Windows.Input;

namespace ClipBoard.Views.Behaviors
{
    public static class ListBoxBehaviors
    {
        public static readonly AttachedProperty<ICommand?> OpenClipProperty = AvaloniaProperty.RegisterAttached<Control, ICommand?>("BeginEdit", typeof(ListBoxBehaviors));

        static ListBoxBehaviors()
        {
            OpenClipProperty.Changed.AddClassHandler<Control>((control, e) =>
            {
                //control.DoubleTapped -= OpenClip;
                //control.DoubleTapped += OpenClip;
            });
        }

        private static void OpenClip(object? sender, TappedEventArgs e)
        {
            if (sender is not Control control) return;

            var command = GetOpenClip(control);

            if (command?.CanExecute(null) == true)
                command.Execute(null);
        }

        public static void SetOpenClip(AvaloniaObject element, ICommand? value) => element.SetValue(OpenClipProperty, value);
        public static ICommand? GetOpenClip(AvaloniaObject element) => element.GetValue(OpenClipProperty);
    }
}
