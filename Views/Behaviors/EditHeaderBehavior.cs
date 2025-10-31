using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ClipBoard.Views.Behaviors
{
    public class EnterKeyBehavior : AvaloniaObject
    {
        public static readonly AttachedProperty<ICommand?> ConfirmCommandProperty = AvaloniaProperty.RegisterAttached<Control, ICommand?>("ConfirmCommand", typeof(EnterKeyBehavior));

        public static readonly AttachedProperty<object?> CommandParameterProperty = AvaloniaProperty.RegisterAttached<Control, object?>("CommandParameter",typeof(EnterKeyBehavior));
        public object? CommandParameter
        {
            get => GetValue(CommandParameterProperty);
            set => SetValue(CommandParameterProperty, value);
        }

        static EnterKeyBehavior()
        {
            ConfirmCommandProperty.Changed.AddClassHandler<Control>((control, _) =>
            {
                control.KeyDown += (s, e) =>
                {
                    if (e.Key == Key.Enter)
                    {
                        var command = GetConfirmCommand(control);
                        var parameter = GetCommandParameter(control);

                        if (command?.CanExecute(parameter) == true)
                            command.Execute(parameter);

                        e.Handled = true;
                    }
                };
            });
        }

        public static void SetConfirmCommand(AvaloniaObject element, ICommand? value) =>
            element.SetValue(ConfirmCommandProperty, value);

        public static ICommand? GetConfirmCommand(AvaloniaObject element) =>
            element.GetValue(ConfirmCommandProperty);

        public static void SetCommandParameter(AvaloniaObject element, object? value) =>
            element.SetValue(CommandParameterProperty, value);

        public static object? GetCommandParameter(AvaloniaObject element) =>
            element.GetValue(CommandParameterProperty);
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

        public static void SetCommand(AvaloniaObject element, ICommand? value) =>
            element.SetValue(CommandProperty, value);

        public static ICommand? GetCommand(AvaloniaObject element) =>
            element.GetValue(CommandProperty);
    }
}
