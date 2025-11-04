using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using ClipBoard.ViewModels;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using static ClipBoard.ViewModels.ClipsViewModel;

namespace ClipBoard.Views.Behaviors
{
    public class KeyBehavior : AvaloniaObject
    {
        public static readonly AttachedProperty<ICommand?> ConfirmCommandProperty = AvaloniaProperty.RegisterAttached<Control, ICommand?>("ConfirmCommand", typeof(KeyBehavior));

        public static readonly AttachedProperty<ICommand?> CancelCommandProperty = AvaloniaProperty.RegisterAttached<Control, ICommand?>("CancelCommand",typeof(KeyBehavior)); 

        static KeyBehavior()
        {
            ConfirmCommandProperty.Changed.AddClassHandler<Control>((control, e) =>
            {
                control.KeyDown -= OnKeyDown;
                control.KeyDown += OnKeyDown;
            });

            CancelCommandProperty.Changed.AddClassHandler<Control>((control, e) =>
            {
                control.KeyDown -= OnKeyDown;
                control.KeyDown += OnKeyDown;
            });
        }
        private static void OnKeyDown(object? sender, KeyEventArgs e)
        {
            if (sender is not Control textBox) return;

            if (e.Key == Key.Enter)
            {
                var command = GetConfirmCommand(textBox);

                if (command != null)
                {
                    command.Execute(command);
                }

                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                var command = GetCancelCommand(textBox);
                if (command?.CanExecute(CancelCommandProperty) == true)
                    command.Execute(CancelCommandProperty);

                e.Handled = true;
            }
        }

        public static void SetConfirmCommand(AvaloniaObject element, ICommand? value) => element.SetValue(ConfirmCommandProperty, value);

        public static ICommand? GetConfirmCommand(AvaloniaObject element) => element.GetValue(ConfirmCommandProperty);

        public static void SetCancelCommand(AvaloniaObject element, ICommand? value) => element.SetValue(CancelCommandProperty, value);

        public static ICommand? GetCancelCommand(AvaloniaObject element) => element.GetValue(CancelCommandProperty);
    }
    public static class DoubleClickBehavior
    {
        public static readonly AttachedProperty<ICommand?> CommandProperty = AvaloniaProperty.RegisterAttached<Control, ICommand?>("Command", typeof(DoubleClickBehavior));

        static DoubleClickBehavior()
        {
            CommandProperty.Changed.AddClassHandler<Control>((control, e) =>
            {
                control.DoubleTapped += (s, args) =>
                {
                    var command = GetCommand(control);
                    if (command?.CanExecute(null) == true)
                        command.Execute(null);
                };
            });
        }

        public static void SetCommand(AvaloniaObject element, ICommand? value) => element.SetValue(CommandProperty, value);

        public static ICommand? GetCommand(AvaloniaObject element) => element.GetValue(CommandProperty);
    }
}
