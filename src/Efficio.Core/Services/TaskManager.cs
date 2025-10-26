using System.Collections.ObjectModel;
using System.Text.Json;
using Efficio.Core.Models;

namespace Efficio.Core.Services;

public class TaskManager
{
    private static TaskManager? _instance;
    private static readonly object _lock = new object();
    
    private static readonly string DataDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "Efficio",
        "Data"
    );
    
    private static readonly string LongTermTasksFile = Path.Combine(DataDirectory, "longtermtasks.json");
    private static readonly string ShortTermTasksFile = Path.Combine(DataDirectory, "shorttermtasks.json");
    
    public ObservableCollection<TaskItem> LongTermTasks { get; private set; }
    public ObservableCollection<TaskItem> ShortTermTasks { get; private set; }
    
    private TaskManager()
    {
        LongTermTasks = new ObservableCollection<TaskItem>();
        ShortTermTasks = new ObservableCollection<TaskItem>();
        
        // Subscribe to collection changes for auto-save
        LongTermTasks.CollectionChanged += OnCollectionChanged;
        ShortTermTasks.CollectionChanged += OnCollectionChanged;
        
        // Ensure data directory exists
        Directory.CreateDirectory(DataDirectory);
        
        Console.WriteLine($"TaskManager initialized. Data directory: {DataDirectory}");
    }
    
    public static TaskManager Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new TaskManager();
                    }
                }
            }
            return _instance;
        }
    }
    
    public void AddTask(TaskItem task)
    {
        Console.WriteLine($"AddTask called: {task.Name}, Type: {task.Type}");
        if (task.Type == TaskType.LongTerm)
        {
            LongTermTasks.Add(task);
            Console.WriteLine($"Added to LongTermTasks. Count: {LongTermTasks.Count}");
        }
        else
        {
            ShortTermTasks.Add(task);
            Console.WriteLine($"Added to ShortTermTasks. Count: {ShortTermTasks.Count}");
        }
    }
    
    public void RemoveTask(TaskItem task)
    {
        if (task.Type == TaskType.LongTerm)
        {
            LongTermTasks.Remove(task);
        }
        else
        {
            ShortTermTasks.Remove(task);
        }
    }
    
    public void UpdateTask(TaskItem task)
    {
        // Since we're using ObservableCollection, property changes will be reflected
        // This method is here for future complex update logic if needed
        // Manually trigger save since property changes don't trigger CollectionChanged
        SaveTasks();
    }
    
    public void LoadTasks()
    {
        try
        {
            // Temporarily unsubscribe from events to avoid triggering saves during load
            LongTermTasks.CollectionChanged -= OnCollectionChanged;
            ShortTermTasks.CollectionChanged -= OnCollectionChanged;
            
            // Load long-term tasks
            if (File.Exists(LongTermTasksFile))
            {
                var json = File.ReadAllText(LongTermTasksFile);
                var tasks = JsonSerializer.Deserialize<List<TaskItem>>(json);
                if (tasks != null)
                {
                    LongTermTasks.Clear();
                    foreach (var task in tasks)
                    {
                        LongTermTasks.Add(task);
                    }
                }
                Console.WriteLine($"Loaded {LongTermTasks.Count} long-term tasks from {LongTermTasksFile}");
            }
            else
            {
                Console.WriteLine($"Long-term tasks file not found: {LongTermTasksFile}");
            }
            
            // Load short-term tasks
            if (File.Exists(ShortTermTasksFile))
            {
                var json = File.ReadAllText(ShortTermTasksFile);
                var tasks = JsonSerializer.Deserialize<List<TaskItem>>(json);
                if (tasks != null)
                {
                    ShortTermTasks.Clear();
                    foreach (var task in tasks)
                    {
                        ShortTermTasks.Add(task);
                    }
                }
                Console.WriteLine($"Loaded {ShortTermTasks.Count} short-term tasks from {ShortTermTasksFile}");
            }
            else
            {
                Console.WriteLine($"Short-term tasks file not found: {ShortTermTasksFile}");
            }
            
            // Re-subscribe to events
            LongTermTasks.CollectionChanged += OnCollectionChanged;
            ShortTermTasks.CollectionChanged += OnCollectionChanged;
        }
        catch (Exception ex)
        {
            // Log error - for now just silently fail
            // TODO: Add proper error logging
            Console.WriteLine($"Error loading tasks: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            
            // Re-subscribe to events even if there's an error
            LongTermTasks.CollectionChanged -= OnCollectionChanged;
            ShortTermTasks.CollectionChanged -= OnCollectionChanged;
            LongTermTasks.CollectionChanged += OnCollectionChanged;
            ShortTermTasks.CollectionChanged += OnCollectionChanged;
        }
    }
    
    private void OnCollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        SaveTasks();
    }
    
    private void SaveTasks()
    {
        try
        {
            Console.WriteLine($"SaveTasks called. LongTerm: {LongTermTasks.Count}, ShortTerm: {ShortTermTasks.Count}");
            
            // Save long-term tasks
            var longTermJson = JsonSerializer.Serialize(LongTermTasks, new JsonSerializerOptions 
            { 
                WriteIndented = true 
            });
            File.WriteAllText(LongTermTasksFile, longTermJson);
            Console.WriteLine($"Saved {LongTermTasks.Count} long-term tasks to {LongTermTasksFile}");
            
            // Save short-term tasks
            var shortTermJson = JsonSerializer.Serialize(ShortTermTasks, new JsonSerializerOptions 
            { 
                WriteIndented = true 
            });
            File.WriteAllText(ShortTermTasksFile, shortTermJson);
            Console.WriteLine($"Saved {ShortTermTasks.Count} short-term tasks to {ShortTermTasksFile}");
        }
        catch (Exception ex)
        {
            // Log error - for now just silently fail
            // TODO: Add proper error logging
            Console.WriteLine($"Error saving tasks: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }
}
