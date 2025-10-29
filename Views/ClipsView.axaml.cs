using Avalonia.Markup.Xaml;
using ReactiveUI.Avalonia;
using ClipBoard.ViewModels;
using ReactiveUI;

namespace ClipBoard.Views;

public partial class ClipsView : ReactiveWindow<ClipsViewModel>
{
    public ClipsView()
    {
        this.WhenActivated(disposables => { });
        AvaloniaXamlLoader.Load(this);
        InitializeComponent();
    }

    private void TabControl_SelectionChanged(object? sender, Avalonia.Controls.SelectionChangedEventArgs e)
    {

    }
}