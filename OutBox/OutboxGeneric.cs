using MessageBrokers.Common.Contracts;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace OutBox;

public class OutboxGeneric<TDbContext> : IMessageBroker where TDbContext : DbContext
{
    private readonly TDbContext _db;
    public OutboxGeneric(TDbContext db) => _db = db;

    public async Task SendMessage<T>(T message, CancellationToken ct = default) where T : IMessage
    {
        var outboxMessage = new OutboxMessage
        {
            Id = message.Id != Guid.Empty ? message.Id : Guid.NewGuid(),
            OccurredOnUtc = DateTime.UtcNow,
            Type = typeof(T).AssemblyQualifiedName ?? typeof(T).FullName ?? typeof(T).Name,
            Content = JsonSerializer.Serialize(message)
        };
        await _db.Set<OutboxMessage>().AddAsync(outboxMessage, ct);
    }
}