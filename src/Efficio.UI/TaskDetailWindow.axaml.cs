using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using Efficio.Core.Models;
using Efficio.Core.Services;

namespace Efficio.UI;

public partial class TaskDetailWindow : Window
{
    private const int AnimationSteps = 30;
    private const int AnimationDelayMs = 8;
    private const double ScreenWidthPercentage = 0.25; // 25% of screen width
    
    private string _taskTitle = string.Empty;
    private string _taskDescription = string.Empty;
    private Window? _parentWindow;
    private TaskItem? _currentTask;
    
    public TaskDetailWindow()
    {
        InitializeComponent();
        
        // Make window borderless and non-resizable
        SystemDecorations = SystemDecorations.None;
        CanResize = false;
        
        // Set window properties for dock behavior
        Topmost = false; // Start as false during animation, will be set to true after animation
        ShowInTaskbar = false; // Don't show in taskbar since it's a secondary window
        
        Opened += OnWindowOpened;
        Deactivated += OnWindowDeactivated;
        LostFocus += OnWindowLostFocus;
    }
    
    private void OnWindowLostFocus(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        // Prevent the window from closing when it loses focus
        Console.WriteLine("Window lost focus - staying open");
    }
    
    private void OnWindowDeactivated(object? sender, EventArgs e)
    {
        // Prevent the window from closing when it loses focus
        // Do nothing here - just let it stay open
        Console.WriteLine("Window deactivated (lost focus) - staying open");
    }
    
    private void OnWindowOpened(object? sender, EventArgs e)
    {
        SlideInFromRight();
    }
    
    public void SetParentWindow(Window parentWindow)
    {
        _parentWindow = parentWindow;
    }
    
    public void SetTaskDetails(TaskItem task)
    {
        _currentTask = task;
        _taskTitle = task.Name;
        _taskDescription = task.Description ?? "No description available.";
        
        // Update the UI
        Dispatcher.UIThread.Post(() =>
        {
            TaskTitleText.Text = _taskTitle;
            TaskDescriptionText.Text = _taskDescription;
        });
    }
    
    // Overload for backward compatibility
    public void SetTaskDetails(string taskTitle, string? description = null)
    {
        _taskTitle = taskTitle;
        _taskDescription = description ?? "No description available.";
        
        // Update the UI
        Dispatcher.UIThread.Post(() =>
        {
            TaskTitleText.Text = _taskTitle;
            TaskDescriptionText.Text = _taskDescription;
        });
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
        
        // Calculate target position: just to the left of the parent window (MainWindow)
        double targetX;
        if (_parentWindow != null)
        {
            // Position just to the left of the parent window
            targetX = _parentWindow.Position.X - targetWidth;
        }
        else
        {
            // Fallback: position at a default location
            targetX = workingArea.Right - (workingArea.Width * 0.45); // 45% from right
        }
        
        // Start off-screen (to the right)
        double startX = workingArea.Right;
        
        Position = new PixelPoint((int)startX, workingArea.Y);
        
        // Animate with easing - slide in from right
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
        
        // Set Topmost to true after animation completes
        Topmost = true;
        
        // Ensure MainWindow stays above this window
        if (_parentWindow != null)
        {
            _parentWindow.Activate();
        }
    }
    
    public async Task SlideOutAndClose()
    {
        var screen = Screens.Primary;
        if (screen == null)
        {
            return;
        }
        
        // Set Topmost to false before sliding out
        Topmost = false;
        
        var workingArea = screen.WorkingArea;
        double startX = Position.X;
        double endX = workingArea.Right;
        int startY = Position.Y;
        
        Console.WriteLine($"Starting slide out animation from X={startX} to X={endX}");
        
        // Animate with easing - slide out to the right
        for (int i = 0; i <= AnimationSteps; i++)
        {
            double t = i / (double)AnimationSteps;
            // Apply ease-in cubic easing for smooth acceleration
            double easedT = EaseInCubic(t);
            double x = startX + (endX - startX) * easedT;
            
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                Position = new PixelPoint((int)x, startY);
            });
            await Task.Delay(AnimationDelayMs);
        }
        
        Console.WriteLine("Slide out animation completed");
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
        Console.WriteLine($"OnClosing called, _isAnimatingOut={_isAnimatingOut}, _isClosingRequested={_isClosingRequested}");
        Console.WriteLine($"Close reason: {e.CloseReason}");
        Console.WriteLine($"Stack trace: {Environment.StackTrace}");
        
        // If we haven't explicitly requested to close, block it
        if (!_isClosingRequested)
        {
            Console.WriteLine("Preventing unexpected close - not explicitly requested");
            e.Cancel = true;
            return;
        }
        
        // We've requested to close - check if animation is complete
        if (_isAnimatingOut)
        {
            Console.WriteLine("Allowing window to close - animation complete");
            base.OnClosing(e);
            return;
        }
        
        // Animation not complete yet, cancel this close attempt
        Console.WriteLine("Animation in progress, canceling close");
        e.Cancel = true;
    }
    
    protected override void OnClosed(EventArgs e)
    {
        Console.WriteLine("OnClosed called");
        Console.WriteLine($"Stack trace: {Environment.StackTrace}");
        base.OnClosed(e);
    }
    
    private bool _isAnimatingOut = false;
    private bool _isClosingRequested = false;
    
    // Method to force close without animation (used when MainWindow closes)
    public void ForceClose()
    {
        _isClosingRequested = true;
        _isAnimatingOut = true;
        Close();
    }
    
    // Public method to close with animation (can be called externally)
    public async Task SlideOutAndCloseAsync()
    {
        if (_isClosingRequested)
        {
            Console.WriteLine("Close already requested, ignoring");
            return;
        }
        
        Console.WriteLine("External close requested - starting animation");
        _isClosingRequested = true;
        
        Console.WriteLine("Starting slide out animation...");
        await SlideOutAndClose();
        
        Console.WriteLine("Animation complete, setting flag and closing...");
        _isAnimatingOut = true;
        
        // Now close for real
        Close();
    }
    
    // Method to request close with animation
    private void RequestCloseWithAnimation()
    {
        if (_isClosingRequested)
        {
            Console.WriteLine("Close already requested, ignoring");
            return;
        }
        
        Console.WriteLine("Close requested - starting animation");
        _isClosingRequested = true;
        
        // Start animation on the UI thread
        Dispatcher.UIThread.Post(async () =>
        {
            Console.WriteLine("Starting slide out animation...");
            await SlideOutAndClose();
            
            Console.WriteLine("Animation complete, setting flag and closing...");
            _isAnimatingOut = true;
            
            // Now close for real
            Close();
        });
    }
    
    // Event handler for delete button
    private async void DeleteButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (_currentTask == null)
        {
            // Show error message if no task is loaded
            var errorDialog = new Window
            {
                Title = "Error",
                Width = 300,
                Height = 150,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                CanResize = false,
                Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#1E1E1E")),
                SystemDecorations = SystemDecorations.Full
            };
            
            var okButton = new Button
            {
                Content = "OK",
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                Padding = new Thickness(20, 5),
                Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#D4AF37")),
                Foreground = Avalonia.Media.Brushes.Black,
                BorderThickness = new Thickness(0)
            };
            okButton.Click += (s, args) => errorDialog.Close();
            
            errorDialog.Content = new StackPanel
            {
                Margin = new Thickness(20),
                Spacing = 15,
                Children =
                {
                    new TextBlock
                    {
                        Text = "No task is currently loaded.",
                        TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                        FontSize = 14,
                        Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#E4E4E7"))
                    },
                    okButton
                }
            };
            
            await errorDialog.ShowDialog(_parentWindow ?? this);
            return;
        }
        
        // Show confirmation dialog
        var confirmDialog = new Window
        {
            Title = "Confirm Deletion",
            Width = 400,
            Height = 220,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            CanResize = false,
            Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#1E1E1E")),
            SystemDecorations = SystemDecorations.Full
        };
        
        bool deleteConfirmed = false;
        
        var deleteButton = new Button
        {
            Content = "Delete",
            Padding = new Thickness(20, 8),
            Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#E81123")),
            Foreground = Avalonia.Media.Brushes.White,
            BorderThickness = new Thickness(0),
            FontWeight = Avalonia.Media.FontWeight.SemiBold
        };
        deleteButton.Click += (s, args) => 
        { 
            deleteConfirmed = true;
            confirmDialog.Close();
        };
        
        var cancelButton = new Button
        {
            Content = "Cancel",
            Padding = new Thickness(20, 8),
            Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#3F3F46")),
            Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#E4E4E7")),
            BorderThickness = new Thickness(0)
        };
        cancelButton.Click += (s, args) => confirmDialog.Close();
        
        confirmDialog.Content = new StackPanel
        {
            Margin = new Thickness(20),
            Spacing = 15,
            Children =
            {
                new TextBlock
                {
                    Text = $"Are you sure you want to delete the task:",
                    FontSize = 14,
                    Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#E4E4E7"))
                },
                new TextBlock
                {
                    Text = $"\"{_currentTask.Name}\"",
                    FontWeight = Avalonia.Media.FontWeight.Bold,
                    FontSize = 14,
                    Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#D4AF37")),
                    TextWrapping = Avalonia.Media.TextWrapping.Wrap
                },
                new TextBlock
                {
                    Text = "This action cannot be undone.",
                    FontSize = 12,
                    Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#A1A1AA"))
                },
                new StackPanel
                {
                    Orientation = Avalonia.Layout.Orientation.Horizontal,
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                    Spacing = 10,
                    Children = { deleteButton, cancelButton }
                }
            }
        };
        
        await confirmDialog.ShowDialog(_parentWindow ?? this);
        
        if (deleteConfirmed)
        {
            // User confirmed deletion
            TaskManager.Instance.RemoveTask(_currentTask);
            
            // Close the detail window with animation
            RequestCloseWithAnimation();
        }
    }
    
    // Event handler for close button
    private void CloseButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        RequestCloseWithAnimation();
    }
}
