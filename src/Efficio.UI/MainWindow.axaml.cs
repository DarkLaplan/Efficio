using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using Efficio.Core.Services;

namespace Efficio.UI;

public partial class MainWindow : Window
{
    private const int AnimationSteps = 30;
    private const int AnimationDelayMs = 8;
    private const double ScreenWidthPercentage = 0.20; // 20% of screen width
    
    private TaskDetailWindow? _taskDetailWindow;
    
    public MainWindow()
    {
        InitializeComponent();
        
        // Load tasks from files on startup
        TaskManager.Instance.LoadTasks();
        
        // Make window borderless and non-resizable
        SystemDecorations = SystemDecorations.None;
        CanResize = false;
        
        // Set window properties for dock behavior
        Topmost = true;
        ShowInTaskbar = true; // Keep in taskbar since this is the main window
        
        Opened += OnWindowOpened;
        
        // Subscribe to task selection events
        LongTermTasksControl.TaskSelected += OnTaskSelected;
        ShortTermTasksControl.TaskSelected += OnTaskSelected;
    }
    
    private void OnTaskSelected(object? sender, string taskTitle)
    {
        // Check if detail window is already open
        bool windowIsOpen = false;
        if (_taskDetailWindow != null)
        {
            try
            {
                // Try to access a property to see if window is still valid
                _ = _taskDetailWindow.IsVisible;
                windowIsOpen = true;
            }
            catch
            {
                // Window is closed or disposed
                windowIsOpen = false;
            }
        }
        
        if (windowIsOpen && _taskDetailWindow != null)
        {
            // Window is already open, just update the data
            _taskDetailWindow.SetTaskDetails(taskTitle, $"Details for: {taskTitle}\n\nThis is where more information about the task would be displayed.");
            _taskDetailWindow.Activate(); // Bring to front if needed
        }
        else
        {
            // Create and show new detail window with animation
            _taskDetailWindow = new TaskDetailWindow();
            _taskDetailWindow.SetParentWindow(this);
            _taskDetailWindow.SetTaskDetails(taskTitle, $"Details for: {taskTitle}\n\nThis is where more information about the task would be displayed.");
            _taskDetailWindow.Show();
        }
        
        // Ensure MainWindow stays on top of TaskDetailWindow
        this.Activate();
    }
    
    private void OnWindowOpened(object? sender, EventArgs e)
    {
        SlideInFromRight();
    }
    
    private async void SlideInFromRight()
    {
        // Get the screen where this window should appear (primary screen)
        var screen = Screens.Primary;
        if (screen == null) return;
        
        var workingArea = screen.WorkingArea;
        
        double targetWidth = workingArea.Width * ScreenWidthPercentage;
        double targetHeight = workingArea.Height;
        
        Width = targetWidth;
        Height = targetHeight;
        
        // Start off-screen (to the right)
        double startX = workingArea.Right;
        double targetX = workingArea.Right - targetWidth;
        
        Position = new PixelPoint((int)startX, workingArea.Y);
        
        // Animate with easing
        for (int i = 0; i <= AnimationSteps; i++)
        {
            double t = i / (double)AnimationSteps;
            // Apply ease-out cubic easing for smooth deceleration
            double easedT = EaseOutCubic(t);
            double x = startX - (startX - targetX) * easedT;
            
            Position = new PixelPoint((int)x, workingArea.Y);
            await Task.Delay(AnimationDelayMs);
        }
        
        // Ensure final position is exact
        Position = new PixelPoint((int)targetX, workingArea.Y);
    }
    
    public async Task SlideOutAndClose()
    {
        var screen = Screens.Primary;
        if (screen == null)
        {
            Close();
            return;
        }
        
        var workingArea = screen.WorkingArea;
        double startX = Position.X;
        double endX = workingArea.Right;
        int startY = Position.Y;
        
        // Animate with easing
        for (int i = 0; i <= AnimationSteps; i++)
        {
            double t = i / (double)AnimationSteps;
            // Apply ease-in cubic easing for smooth acceleration
            double easedT = EaseInCubic(t);
            double x = startX + (endX - startX) * easedT;
            
            Position = new PixelPoint((int)x, startY);
            await Task.Delay(AnimationDelayMs);
        }
        
        Close();
    }
    
    // Easing functions for smooth animations
    private static double EaseOutCubic(double t)
    {
        return 1 - Math.Pow(1 - t, 3);
    }
    
    private static double EaseInCubic(double t)
    {
        return t * t * t;
    }
    
    // Call this method to close with animation
    protected override void OnClosing(WindowClosingEventArgs e)
    {
        // Close the task detail window if it's open
        if (_taskDetailWindow != null)
        {
            try
            {
                _taskDetailWindow.Close();
            }
            catch
            {
                // Window might already be closed
            }
        }
        
        // Prevent immediate close if animation hasn't run
        if (!_isAnimatingOut)
        {
            e.Cancel = true;
            _isAnimatingOut = true;
            _ = SlideOutAndClose();
        }
        
        base.OnClosing(e);
    }
    
    private bool _isAnimatingOut = false;
    
    // Event handler for close button
    private void CloseButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Close();
    }
    
    // Event handler for add task button
    private async void AddTaskButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var addTaskWindow = new AddTaskWindow
        {
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };
        
        await addTaskWindow.ShowDialog(this);
    }
}
