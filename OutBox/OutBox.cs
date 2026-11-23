using MessageBrokers.Common.Contracts;
using System.Text.Json;

namespace OutBox;

public class OutBox : IMessageBrokerWithOutbox
{
    private readonly IOutboxDbContext _dbContext;

    public OutBox(IOutboxDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task SendMessage<T>(T message, CancellationToken cancellationToken = default) where T : IMessage
    {
        ArgumentNullException.ThrowIfNull(message);

        var outboxMessage = new OutboxMessage
        {
            Id = message.Id != Guid.Empty ? message.Id : Guid.NewGuid(),
            OccurredOnUtc = DateTime.UtcNow,
            Type = typeof(T).AssemblyQualifiedName ?? typeof(T).FullName ?? typeof(T).Name,
            Content = JsonSerializer.Serialize(message)
        };

        await _dbContext.OutboxMessages.AddAsync(outboxMessage, cancellationToken);
    }
}
