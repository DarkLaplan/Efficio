using System;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Efficio.UI.Controls;

public partial class ShortTermTasksControl : UserControl
{
    public event EventHandler<string>? TaskSelected;
    
    public ShortTermTasksControl()
    {
        InitializeComponent();
        ShortTermListBox.SelectionChanged += OnSelectionChanged;
    }
    
    private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (ShortTermListBox.SelectedItem is ListBoxItem selectedItem)
        {
            var taskTitle = selectedItem.Content?.ToString() ?? "Unknown Task";
            TaskSelected?.Invoke(this, taskTitle);
        }
    }
}
