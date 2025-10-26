using Avalonia.Controls;
using Avalonia.Interactivity;
using Efficio.Core.Models;
using Efficio.Core.Services;

namespace Efficio.UI;

public partial class AddTaskWindow : Window
{
    public bool WasAccepted { get; private set; }
    public TaskItem? CreatedTask { get; private set; }
    
    public AddTaskWindow()
    {
        InitializeComponent();
    }
    
    private void AcceptButton_Click(object? sender, RoutedEventArgs e)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(TaskNameTextBox.Text))
        {
            // Show error - for now just return
            // TODO: Add proper error notification
            return;
        }
        
        // Create the task
        var taskType = TaskTypeComboBox.SelectedIndex == 0 
            ? TaskType.ShortTerm 
            : TaskType.LongTerm;
        
        CreatedTask = new TaskItem
        {
            Name = TaskNameTextBox.Text?.Trim() ?? string.Empty,
            Description = TaskDescriptionTextBox.Text?.Trim() ?? string.Empty,
            Type = taskType,
            CreatedDate = DateTime.Now,
            IsCompleted = false
        };
        
        // Add task to the TaskManager
        TaskManager.Instance.AddTask(CreatedTask);
        
        WasAccepted = true;
        Close();
    }
    
    private void CancelButton_Click(object? sender, RoutedEventArgs e)
    {
        WasAccepted = false;
        Close();
    }
}
