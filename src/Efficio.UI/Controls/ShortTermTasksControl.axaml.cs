using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Efficio.Core.Models;
using Efficio.Core.Services;

namespace Efficio.UI.Controls;

public partial class ShortTermTasksControl : UserControl
{
    public event EventHandler<TaskItem>? TaskSelected;
    
    public ShortTermTasksControl()
    {
        InitializeComponent();
        DataContext = TaskManager.Instance;
        ShortTermListBox.SelectionChanged += OnSelectionChanged;
    }
    
    private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (ShortTermListBox.SelectedItem is TaskItem selectedTask)
        {
            TaskSelected?.Invoke(this, selectedTask);
        }
    }
}
