using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using System.Windows.Input;

namespace ClipBoard.Views.Behaviors
{
    public static class TextBoxBehaviors
    {
        public static readonly AttachedProperty<ICommand?> BeginRenameProperty = AvaloniaProperty.RegisterAttached<Control, ICommand?>("BeginRename", typeof(TextBoxBehaviors));
        public static readonly AttachedProperty<ICommand?> ConfirmRenameProperty = AvaloniaProperty.RegisterAttached<Control, ICommand?>("ConfirmRename", typeof(TextBoxBehaviors));
        public static readonly AttachedProperty<ICommand?> CancelRenameProperty = AvaloniaProperty.RegisterAttached<Control, ICommand?>("CancelRename", typeof(TextBoxBehaviors));
        public static readonly AttachedProperty<bool> IsFocusedProperty = AvaloniaProperty.RegisterAttached<Control, bool>("IsFocused", typeof(TextBoxBehaviors));

        static TextBoxBehaviors()
        {
            BeginRenameProperty.Changed.AddClassHandler<Control>((control, e) =>
            {
                control.DoubleTapped -= OnBeginRename;
                control.DoubleTapped += OnBeginRename;
            });

            ConfirmRenameProperty.Changed.AddClassHandler<Control>((control, e) =>
            {
                control.AttachedToVisualTree -= OnAttached;
                control.AttachedToVisualTree += OnAttached;

                control.DetachedFromVisualTree -= OnDetached;
                control.DetachedFromVisualTree += OnDetached;

                control.KeyDown -= OnKeyDown;
                control.KeyDown += OnKeyDown;

                control.LostFocus -= OnLostFocus;
                control.LostFocus += OnLostFocus;
            });

            CancelRenameProperty.Changed.AddClassHandler<Control>((control, e) =>
            {
                control.KeyDown -= OnKeyDown;
                control.KeyDown += OnKeyDown;
            });

            IsFocusedProperty.Changed.AddClassHandler<Control>((control, e) =>
            {
                if (control is TextBox tb && e ?.NewValue is bool && (bool)e.NewValue)
                {
                    Dispatcher.UIThread.Post(() =>
                    {
                            tb.Focus();
                            tb.SelectAll();
                    }, DispatcherPriority.Background);
                }
            });
        }

        private static void OnBeginRename(object? sender, TappedEventArgs e)
        {
            if (sender is not Control control) return;

            BeginRename(control);
        }
        private static void OnAttached(object? sender, VisualTreeAttachmentEventArgs e)
        {
            if (sender is not TextBox tb) return;

            var window = tb.GetVisualRoot() as Window;
            if (window != null)
            {
                window.PointerPressed += OnWindowClick;
            }
        }
        private static void OnDetached(object? sender, VisualTreeAttachmentEventArgs e)
        {
            if (sender is not TextBox tb) return;

            var window = tb.GetVisualRoot() as Window;
            if (window == null) return;

            window.PointerPressed -= OnWindowClick;
        }
        private static void OnWindowClick(object? sender, PointerPressedEventArgs e)
        {
            if (sender is not Window window) return;

            var focused = window.FocusManager?.GetFocusedElement() as TextBox;
            if (focused == null) return;

            ConfirmRename(focused);
        }
        private static void OnLostFocus(object? sender, RoutedEventArgs e)
        {
            if (sender is not TextBox tb) return;
            ConfirmRename(tb);
        }

        private static void OnKeyDown(object? sender, KeyEventArgs e)
        {
            if (sender is not TextBox textBox) return;

            if (e.Key == Key.Enter)
            {
                textBox.LostFocus -= OnLostFocus;
                ConfirmRename(textBox);
            }
            else if (e.Key == Key.Escape)
            {
                CancelRename(textBox);
            }
        }
        private static void BeginRename(Control tb)
        {
            var command = GetBeginRename(tb);

            if (command?.CanExecute(command) == true)
                command.Execute(command);
        }
        private static void ConfirmRename(TextBox tb)
        {
            var command = GetConfirmRename(tb);

            if (command?.CanExecute(command) == true)
                command.Execute(command);
        }
        private static void CancelRename(TextBox tb)
        {
            var command = GetCancelRename(tb);
            if (command?.CanExecute(command) == true)
                command.Execute(command);
        }

        public static void SetBeginRename(AvaloniaObject element, ICommand? value) => element.SetValue(BeginRenameProperty, value);
        public static ICommand? GetBeginRename(AvaloniaObject element) => element.GetValue(BeginRenameProperty);

        public static void SetConfirmRename(AvaloniaObject element, ICommand? value) => element.SetValue(ConfirmRenameProperty, value);
        public static ICommand? GetConfirmRename(AvaloniaObject element) => element.GetValue(ConfirmRenameProperty);

        public static void SetCancelRename(AvaloniaObject element, ICommand? value) => element.SetValue(CancelRenameProperty, value);
        public static ICommand? GetCancelRename(AvaloniaObject element) => element.GetValue(CancelRenameProperty);

        public static void SetIsFocused(AvaloniaObject element, bool value) => element.SetValue(IsFocusedProperty, value);
        public static bool GetIsFocused(AvaloniaObject element) => element.GetValue(IsFocusedProperty);
    }
}
