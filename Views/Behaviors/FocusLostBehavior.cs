using Avalonia;
using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ClipBoard.Views.Behaviors
{
    public class FocusLostBehavior : AvaloniaObject
    {
        public static readonly AttachedProperty<ICommand?> ConfirmCommandProperty = AvaloniaProperty.RegisterAttached<Control, ICommand?>("ConfirmCommand", typeof(FocusLostBehavior));

        static FocusLostBehavior()
        {
            ConfirmCommandProperty.Changed.AddClassHandler<Control>((control, e) =>
            {
                control.LostFocus -= OnLostFocus; // ensure no duplicates
                control.LostFocus += OnLostFocus;
            });
        }

        private static void OnLostFocus(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (sender is Control control)
            {
                var command = GetConfirmCommand(control);
                var parameter = control.DataContext;

                if (command?.CanExecute(parameter) == true)
                    command.Execute(parameter);
            }
        }

        public static void SetConfirmCommand(AvaloniaObject element, ICommand? value) => element.SetValue(ConfirmCommandProperty, value);

        public static ICommand? GetConfirmCommand(AvaloniaObject element) => element.GetValue(ConfirmCommandProperty);
    }
}
