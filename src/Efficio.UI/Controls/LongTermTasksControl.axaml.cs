using System;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Efficio.UI.Controls;

public partial class LongTermTasksControl : UserControl
{
    public event EventHandler<string>? TaskSelected;
    
    public LongTermTasksControl()
    {
        InitializeComponent();
        LongTermListBox.SelectionChanged += OnSelectionChanged;
    }
    
    private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (LongTermListBox.SelectedItem is ListBoxItem selectedItem)
        {
            var taskTitle = selectedItem.Content?.ToString() ?? "Unknown Task";
            TaskSelected?.Invoke(this, taskTitle);
        }
    }
}
