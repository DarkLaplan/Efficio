using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Efficio.Core.Models;
using Efficio.Core.Services;

namespace Efficio.UI.Controls;

public partial class LongTermTasksControl : UserControl
{
    public event EventHandler<TaskItem>? TaskSelected;
    
    public LongTermTasksControl()
    {
        InitializeComponent();
        DataContext = TaskManager.Instance;
        LongTermListBox.SelectionChanged += OnSelectionChanged;
    }
    
    private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (LongTermListBox.SelectedItem is TaskItem selectedTask)
        {
            TaskSelected?.Invoke(this, selectedTask);
        }
    }
}
