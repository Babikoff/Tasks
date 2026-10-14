namespace TaskService.Domain;

public enum Priority
{
    None = 0,
    Low = 1,
    Medium = 4,
    High = 8
}

public class TaskItem
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty; // не null, не пустой, максимум 200
    public bool IsCompleted { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; } // когда завершили
    public Priority Priority { get; set; } // Low, Medium, High
}
