using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;

namespace Efficio.UI;

public partial class TaskDetailWindow : Window
{
    private const int AnimationSteps = 30;
    private const int AnimationDelayMs = 8;
    private const double ScreenWidthPercentage = 0.25; // 25% of screen width
    
    private string _taskTitle = string.Empty;
    private string _taskDescription = string.Empty;
    private Window? _parentWindow;
    
    public TaskDetailWindow()
    {
        InitializeComponent();
        
        // Make window borderless and non-resizable
        SystemDecorations = SystemDecorations.None;
        CanResize = false;
        
        // Set window properties for dock behavior
        Topmost = false; // Don't set as topmost so MainWindow can be above it
        ShowInTaskbar = false; // Don't show in taskbar since it's a secondary window
        
        Opened += OnWindowOpened;
    }
    
    private void OnWindowOpened(object? sender, EventArgs e)
    {
        SlideInFromRight();
    }
    
    public void SetParentWindow(Window parentWindow)
    {
        _parentWindow = parentWindow;
    }
    
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
        
        // Animate with easing - slide out to the right
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
}
