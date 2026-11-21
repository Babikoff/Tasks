using Common.Contracts;
using TaskService.Models;

[Obsolete]
public class RabbitMqSerivceDummy : IMessageBroker
{
    public async Task SendMessage<T>(T message, CancellationToken cancellationToken = default) where T : IMessage
    {
        return; 
    }
}