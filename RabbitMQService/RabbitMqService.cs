using System.Text;
using System.Text.Json;
using MessageBrokers.Common.Contracts;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitMqService
{
    /// <summary>
    /// Реализация интерфейса работы с очередью <see cref="IDocumentProcessingQueueService"/> на основе RabbitMQ.
    /// </summary>
    public class RabbitMqService: IMessageBroker
    {
        private readonly IConnection _connection;
        private readonly string _queueName;
        private readonly ILogger<RabbitMqService> _logger;

        public RabbitMqService(IConnection connection, string queueName, ILogger<RabbitMqService> logger)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _queueName = queueName ?? throw new ArgumentNullException(nameof(queueName));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        /// <summary>
        /// Отправляет <see cref="QueueDocumentDto"/> в очередь.
        /// </summary>
        public async Task SendMessage<T>(T message, CancellationToken cancellationToken = default) where T : IMessage
        {
            ArgumentNullException.ThrowIfNull(message);

            await using var channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);
            await DeclareQueue(channel, cancellationToken);

            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

            var properties = new BasicProperties
            {
                ContentType = "application/json",
                Persistent = true,
                DeliveryMode = DeliveryModes.Persistent
            };

            await channel.BasicPublishAsync(
                exchange: string.Empty,
                routingKey: _queueName,
                mandatory: false,
                basicProperties: properties,
                body: body,
                cancellationToken: cancellationToken);

            _logger.LogInformation("Published message to queue '{QueueName}': {FileName}", _queueName, message.Id);
        }

        /// <summary>
        /// Запускает процесс прослушивания очереди и получения сообщений из неё.
        /// </summary>
        /// <param name="onMessageAsync">Callback для получения сообщений в вызываемом коде.</param>
        /// <param name="cancellationToken"></param>
        public async Task ConsumeAsync<T>(Func<T, Task> onMessageAsync, CancellationToken cancellationToken = default) where T : IMessage
        {
            ArgumentNullException.ThrowIfNull(onMessageAsync);

            await using var channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);
            await DeclareQueue(channel, cancellationToken);

            _logger.LogInformation("Awaiting messages from queue '{QueueName}'.", _queueName);

            // Настраиваем режим получения сообщений
            await channel.BasicQosAsync(
                prefetchSize: 0,
                prefetchCount: 1, // Будем получать только по одному сообщению за раз
                global: false,
                cancellationToken: cancellationToken
                );

            // Подписываемся на получение сообщений
            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (sender, eventArgs) =>
            {
                try
                {
                    var body = eventArgs.Body.ToArray();
                    var dto = JsonSerializer.Deserialize<T>(body);

                    if (dto != null)
                    {
                        _logger.LogInformation("Received message from queue '{QueueName}'. Message Id: {Id}", _queueName, dto.Id);

                        if (MessageBrokers.Common.Constants.TestQueueMessageConsumingDelay > 0)
                        {
                            await Task.Delay(MessageBrokers.Common.Constants.TestQueueMessageConsumingDelay);
                        }

                        // Вызываем callback обработки payload-а сообщения
                        await onMessageAsync(dto);

                        // Подтверждаем получение сообщения
                        await channel.BasicAckAsync(eventArgs.DeliveryTag, multiple: false, cancellationToken: cancellationToken);
                    }
                    else
                    {
                        _logger.LogWarning("Deserialized DTO is null. Queue '{QueueName}'", _queueName);
                        // Отбрасываем неформатные сообщения без повторных попыток получения
                        await channel.BasicNackAsync(eventArgs.DeliveryTag, multiple: false, requeue: false, cancellationToken: cancellationToken);
                    }
                }
                catch (OperationCanceledException)
                {
                    // При штатном завершении отбрасываем сообщения с опцией повторной попытки получения (при следующем запуске приложения)
                    await channel.BasicNackAsync(eventArgs.DeliveryTag, multiple: false, requeue: true, CancellationToken.None);
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing message from queue '{QueueName}'. DeliveryTag: {DeliveryTag}", _queueName, eventArgs.DeliveryTag);
                    // При ошибке отбрасываем сообщения с опцией повторной попытки получения 
                    await channel.BasicNackAsync(eventArgs.DeliveryTag, multiple: false, requeue: true, CancellationToken.None);
                }
            };

            // Запускаем процесс ожидания и приёма сообщений 
            _logger.LogInformation("Starting event-driven consumer for queue '{QueueName}'", _queueName);
            await channel.BasicConsumeAsync(
                queue: _queueName,
                autoAck: false, // We manage ack/nack manually
                consumer: consumer,
                cancellationToken: cancellationToken);

            // Блокируем выход из метода до получения cancellationToken
            try
            {
                await Task.Delay(Timeout.Infinite, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                // Получен cancellationToken
            }

            _logger.LogInformation("Stopped consuming from queue '{QueueName}'", _queueName);
        }

        private async Task DeclareQueue(IChannel channel, CancellationToken cancellationToken)
        {
            await channel.QueueDeclareAsync(
                queue: _queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null,
                cancellationToken: cancellationToken);
        }

    }
}