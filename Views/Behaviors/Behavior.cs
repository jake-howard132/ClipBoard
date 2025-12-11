//using Avalonia;
//using Avalonia.Controls;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Windows.Input;

//namespace ClipBoard.Views.Behaviors
//{
//    public abstract class Behavior<TEventArgs> where TEventArgs : EventArgs
//    {
//        private readonly Action<Control, EventHandler<TEventArgs>> _attach;
//        private readonly Action<Control, EventHandler<TEventArgs>> _detach;

//        protected Behavior(
//            Action<Control, EventHandler<TEventArgs>> attach,
//            Action<Control, EventHandler<TEventArgs>> detach)
//        {
//            _attach = attach;
//            _detach = detach;
//        }

//        protected AttachedProperty<ICommand?> Register(string propertyName)
//        {
//            var prop = AvaloniaProperty.RegisterAttached<Control, ICommand?>(propertyName, GetType());

//            prop.Changed.Subscribe(args =>
//            {
//                if (args.Sender is Control control)
//                {
//                    _detach(control, OnEvent);
//                    if (args.NewValue is ICommand)
//                        _attach(control, OnEvent);
//                }
//            });

//            return prop;
//        }

//        private void OnEvent(object? sender, TEventArgs e)
//        {
//            if (sender is Control ctrl)
//            {
//                var command = GetCommand(ctrl);
//                if (command?.CanExecute(e) == true)
//                    command.Execute(e);
//            }
//        }

//        public abstract ICommand? GetCommand(AvaloniaObject obj);
//    }
//}
