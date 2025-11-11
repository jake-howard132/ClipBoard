using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using System.Windows.Input;

namespace ClipBoard.Views.Behaviors
{
    public static class ListViewBehaviors
    {
        public static readonly AttachedProperty<ICommand?> OpenClipProperty = AvaloniaProperty.RegisterAttached<Control, ICommand?>("BeginEdit", typeof(ListViewBehaviors));

        static ListViewBehaviors()
        {
            OpenClipProperty.Changed.AddClassHandler<Control>((control, e) =>
            {
                control.DoubleTapped -= OpenClip;
                control.DoubleTapped += OpenClip;
            });
        }

        private static void OpenClip(object? sender, TappedEventArgs e)
        {
            if (sender is not Control control) return;

            var command = GetOpenClip(control);

            if (command?.CanExecute(command) == true)
                command.Execute(command);

            e.Handled = true;
        }

        public static void SetOpenClip(AvaloniaObject element, ICommand? value) => element.SetValue(OpenClipProperty, value);
        public static ICommand? GetOpenClip(AvaloniaObject element) => element.GetValue(OpenClipProperty);
    }
}
