using Avalonia.Controls;
using Avalonia.Xaml.Interactivity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClipBoard.Views.Behaviors
{
    public class FocusOnAttachedBehavior : Behavior<TextBox>
    {
        protected override void OnAttached()
        {
            base.OnAttached();

            if (AssociatedObject == null)
                return;

            AssociatedObject.AttachedToVisualTree += (_, _) =>
            {
                AssociatedObject.Focus();
                AssociatedObject.CaretIndex = int.MaxValue;
                AssociatedObject.SelectAll();
            };
        }
    }
}
