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
    public static class EnterKeyBehavior
    {
        public static readonly AttachedProperty<ICommand> ConfirmCommandProperty = AvaloniaProperty.RegisterAttached<Control, ICommand>("ConfirmCommand", typeof(EnterKeyBehavior));

        static EnterKeyBehavior()
        {
            ConfirmCommandProperty.Changed.AddClassHandler<Control>((control, _) =>
            {
                control.KeyDown += (s, e) =>
                {
                    if (e.Key == Key.Enter)
                    {
                        var command = GetConfirmCommand(control); // Already ICommand?

                        if (command?.CanExecute(null) == true)
                            command.Execute(null);
                    }
                };
            });
        }

        public static void SetConfirmCommand(AvaloniaObject element, ICommand value)
            => element.SetValue(ConfirmCommandProperty, value);

        public static ICommand GetConfirmCommand(AvaloniaObject element)
            => element.GetValue(ConfirmCommandProperty);
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
