namespace OutBox;

public class OutboxMessage
{
    public Guid Id { get; set; }
    public DateTime OccurredOnUtc { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public bool Processed { get; set; } = false;
    public DateTime? ProcessedOnUtc { get; set; }
    public string? Error { get; set; }
}
