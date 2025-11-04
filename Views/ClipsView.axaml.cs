using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using ClipBoard.ViewModels;
using ReactiveUI;
using ReactiveUI.Avalonia;

namespace ClipBoard.Views;

public partial class ClipsView : ReactiveWindow<ClipsViewModel>
{
    public ClipsView()
    {
        this.WhenActivated(disposables =>
        {
            this.ViewModel?.LoadGroupsCommand.Execute();
        });
        AvaloniaXamlLoader.Load(this);
        InitializeComponent();
    }

    private void Window_Closing(object? sender, WindowClosingEventArgs e)
    {
        e.Cancel = true;
        this.Hide();
    }
}
