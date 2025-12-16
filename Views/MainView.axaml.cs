using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using ClipBoard.ViewModels;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using ReactiveUI;
using ReactiveUI.Avalonia;
using System;

namespace ClipBoard.Views;

public partial class MainView : ReactiveWindow<ClipsViewModel>
{
    public MainView()
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
        });
        this.Initialized += (sender, args) =>
        {
            if (this.ViewModel is not ClipsViewModel vm) return;

            vm.LoadClipGroupsCommand.Execute();
        };
    }
    private void Item_DoubleTapped(object? sender, TappedEventArgs e)
    {
        if (sender is not Control control || control.DataContext is not ClipBoard.ViewModels.Clip clip) return;

        clip.OpenClipCommand.Execute();
    }

    private void Window_Closing(object? sender, WindowClosingEventArgs e)
    {
        this.Hide();
        e.Cancel = true;
    }
}
