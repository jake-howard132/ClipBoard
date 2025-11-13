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
        public static readonly AttachedProperty<ICommand?> BeginEditProperty = AvaloniaProperty.RegisterAttached<Control, ICommand?>("BeginEdit", typeof(TextBoxBehaviors));
        public static readonly AttachedProperty<ICommand?> ConfirmEditProperty = AvaloniaProperty.RegisterAttached<Control, ICommand?>("ConfirmEdit", typeof(TextBoxBehaviors));
        public static readonly AttachedProperty<ICommand?> CancelEditProperty = AvaloniaProperty.RegisterAttached<Control, ICommand?>("CancelEdit", typeof(TextBoxBehaviors));
        public static readonly AttachedProperty<bool> IsFocusedProperty = AvaloniaProperty.RegisterAttached<Control, bool>("IsFocused", typeof(TextBoxBehaviors));

        static TextBoxBehaviors()
        {
            BeginEditProperty.Changed.AddClassHandler<Control>((control, e) =>
            {
                control.DoubleTapped -= BeginEdit;
                control.DoubleTapped += BeginEdit;
            });

            IsFocusedProperty.Changed.AddClassHandler<Control>((control, e) =>
            {
                if (e?.NewValue is bool && (bool)e.NewValue)
                {
                    // Delay to ensure TextBox is loaded and visible
                    Dispatcher.UIThread.Post(() =>
                    {
                        if (control is TextBox tb)
                        {
                            tb.Focus();
                            tb.SelectAll();
                        }
                    }, DispatcherPriority.Background);
                }
            });

            ConfirmEditProperty.Changed.AddClassHandler<Control>((control, e) =>
            {
                control.AttachedToVisualTree -= OnAttached;
                control.AttachedToVisualTree += OnAttached;

                control.DetachedFromVisualTree -= OnDetached;
                control.DetachedFromVisualTree += OnDetached;
            });

            ConfirmEditProperty.Changed.AddClassHandler<Control>((control, e) =>
            {
                control.KeyDown -= OnKeyDown;
                control.KeyDown += OnKeyDown;

                control.LostFocus -= OnLostFocus;
                control.LostFocus += OnLostFocus;
            });

            CancelEditProperty.Changed.AddClassHandler<Control>((control, e) =>
            {
                control.KeyDown -= OnKeyDown;
                control.KeyDown += OnKeyDown;
            });
        }

        private static void BeginEdit(object? sender, TappedEventArgs e)
        {
            if (sender is not Control control) return;

            Edit(control);

            e.Handled = true;
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

            Confirm(focused);

            e.Handled = true;
        }
        private static void OnLostFocus(object? sender, RoutedEventArgs e)
        {
            if (sender is not TextBox tb) return;
            Confirm(tb);

            e.Handled = true;
        }

        private static void OnKeyDown(object? sender, KeyEventArgs e)
        {
            if (sender is not TextBox textBox) return;

            if (e.Key == Key.Enter)
            {
                Confirm(textBox);
            }
            else if (e.Key == Key.Escape)
            {
                Cancel(textBox);
            }

            e.Handled = true;
        }
        private static void Edit(Control tb)
        {
            var command = GetBeginEdit(tb);

            if (command?.CanExecute(command) == true)
                command.Execute(command);
        }
        private static void Confirm(TextBox tb)
        {
            var command = GetConfirmEdit(tb);

            if (command?.CanExecute(command) == true)
                command.Execute(command);
        }

        private static void Cancel(TextBox tb)
        {
            var command = GetCancelEdit(tb);
            if (command?.CanExecute(command) == true)
                command.Execute(command);
        }

        public static void SetBeginEdit(AvaloniaObject element, ICommand? value) => element.SetValue(BeginEditProperty, value);
        public static ICommand? GetBeginEdit(AvaloniaObject element) => element.GetValue(BeginEditProperty);

        public static void SetIsFocused(AvaloniaObject element, bool value) => element.SetValue(IsFocusedProperty, value);
        public static bool GetIsFocused(AvaloniaObject element) => element.GetValue(IsFocusedProperty);

        public static void SetConfirmEdit(AvaloniaObject element, ICommand? value) => element.SetValue(ConfirmEditProperty, value);
        public static ICommand? GetConfirmEdit(AvaloniaObject element) => element.GetValue(ConfirmEditProperty);

        public static void SetCancelEdit(AvaloniaObject element, ICommand? value) => element.SetValue(CancelEditProperty, value);
        public static ICommand? GetCancelEdit(AvaloniaObject element) => element.GetValue(CancelEditProperty);
    }
}
