using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using ClipBoard.ViewModels;
using ReactiveUI;
using ReactiveUI.Avalonia;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Input;

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

    private void AddClipGroup_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        var tabControl = ((TabItem)sender).GetVisualAncestors().OfType<TabControl>().FirstOrDefault();
        tabControl.Items.Insert(tabControl.Items.Count - 1, new ClipGroup(default(Guid), "New Group", null, new List<Clip>(), tabControl.Items.Count - 1));
        var newClipGroup = (TabItem)tabControl.Items.GetAt(tabControl.Items.Count - 1);
        tabControl.SelectedItem = newClipGroup;
        newClipGroup.Focus();
    }

    private void TextBox_GotFocus(object? sender, GotFocusEventArgs e)
    {
        if (sender is TextBox tb)
        {
            tb.SelectAll();
        }
    }
}