using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Avalonia.VisualTree;
using ClipBoard.ViewModels;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using ReactiveUI;
using ReactiveUI.Avalonia;

namespace ClipBoard.Views;

public partial class ClipsView : ReactiveWindow<ClipsViewModel>
{
    public ClipsView()
    {
        this.WhenActivated(disposables =>
        {
            InitializeComponent();

            if (this.ViewModel is not ClipsViewModel vm) return;

            vm.ConfirmDelete.RegisterHandler(async interaction =>
            {
                var result = await MessageBoxManager.GetMessageBoxStandard("Are you sure?", interaction.Input, ButtonEnum.YesNo).ShowAsync();

                interaction.SetOutput(result == ButtonResult.Yes);
            });

            vm.LoadClipGroupsCommand.Execute();
        });
    }

    private void Window_Closing(object? sender, WindowClosingEventArgs e)
    {
        this.Hide();
        e.Cancel = true;
    }
}
