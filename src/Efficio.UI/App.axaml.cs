using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;
using System;
using System.IO;

namespace Efficio.UI;

public partial class App : Application
{
    private TrayIcon? _tray;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            try
            {
                var assets = AssetLoader.Open(new Uri("avares://Efficio.UI/Assets/app.ico"));
                
                var openMenuItem = new NativeMenuItem("Open");
                openMenuItem.Click += (s, e) => ShowMainWindow(desktop);
                
                var exitMenuItem = new NativeMenuItem("Exit");
                exitMenuItem.Click += (s, e) => desktop.Shutdown();

                _tray = new TrayIcon
                {
                    Icon = new WindowIcon(assets),
                    ToolTipText = "Efficio",
                    Menu = new NativeMenu
                    {
                        Items = { openMenuItem, exitMenuItem }
                    }
                };
                
                // Also handle left-click to open window
                _tray.Clicked += (s, e) => ShowMainWindow(desktop);
                _tray.IsVisible = true;
                
                // Show window on first start
                ShowMainWindow(desktop);
            }
            catch (Exception ex)
            {
                // If tray icon fails (e.g., missing icon file), just show the main window
                System.Diagnostics.Debug.WriteLine($"Tray icon failed: {ex.Message}");
                desktop.MainWindow = new MainWindow();
                desktop.MainWindow.Show();
            }
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void ShowMainWindow(IClassicDesktopStyleApplicationLifetime desktop)
    {
        if (desktop.MainWindow == null)
        {
            desktop.MainWindow = new MainWindow();
        }

        desktop.MainWindow.Show();
        desktop.MainWindow.Activate();
    }
}