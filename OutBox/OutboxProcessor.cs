using System.Text.Json;
using MessageBrokers.Common.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace OutBox;

public class OutboxProcessor : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OutboxProcessor> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromSeconds(5);

    public OutboxProcessor(IServiceProvider serviceProvider, ILogger<OutboxProcessor> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Outbox processor background service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessMessagesAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Error occurred while processing outbox messages.");
            }

            try
            {
                await Task.Delay(_interval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        _logger.LogInformation("Outbox processor background service stopped.");
    }

    private async Task ProcessMessagesAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IOutboxDbContext>();
        var targetBroker = scope.ServiceProvider.GetRequiredService<IMessageBroker>();

        var messages = await dbContext.OutboxMessages
            .Where(m => !m.Processed)
            .OrderBy(m => m.OccurredOnUtc)
            .Take(20)
            .ToListAsync(cancellationToken);

        if (messages.Count == 0)
        {
            return;
        }

        foreach (var message in messages)
        {
            try
            {
                var messageType = Type.GetType(message.Type);
                if (messageType == null)
                {
                    _logger.LogError("Could not find type {Type} for outbox message {Id}", message.Type, message.Id);
                    message.Error = $"Type {message.Type} could not be resolved.";
                    message.ProcessedOnUtc = DateTime.UtcNow;
                    continue;
                }

                var deserialized = JsonSerializer.Deserialize(message.Content, messageType);
                if (deserialized is IMessage typedMessage)
                {
                    var sendMethod = typeof(RabbitMqService.RabbitMqService)
                        .GetMethod(nameof(RabbitMqService.RabbitMqService.SendMessage))!
                        .MakeGenericMethod(messageType);

                    var task = (Task)sendMethod.Invoke(targetBroker, new object[] { typedMessage, cancellationToken })!;
                    await task;
                    message.Error = null;
                }
                else
                {
                    message.Error = "Deserialized payload is not IMessage.";
                }
                message.Processed = true;
                message.ProcessedOnUtc = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish outbox message {Id}", message.Id);
                message.Error = ex.ToString();
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
