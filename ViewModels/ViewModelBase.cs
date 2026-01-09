using ReactiveUI;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;

namespace ClipBoard.ViewModels
{
    public class ViewModelBase : ReactiveObject, IActivatableViewModel
    {
        public string Title => "Design Preview";

        public ViewModelActivator Activator { get; } = new ViewModelActivator();

        public ViewModelBase()
        {
            this.WhenActivated(disposables =>
            {
                /* Handle activation */
                Disposable
                    .Create(() => { /* Handle deactivation */ })
                    .DisposeWith(disposables);
            });
        }
    }
}
