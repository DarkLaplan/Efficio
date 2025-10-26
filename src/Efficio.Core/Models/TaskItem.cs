namespace Efficio.Core.Models;

public class TaskItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime? DueDate { get; set; }
    public bool IsCompleted { get; set; }
    public TaskType Type { get; set; }
    public string Notes { get; set; } = string.Empty;
}

public enum TaskType
{
    ShortTerm,
    LongTerm
}
