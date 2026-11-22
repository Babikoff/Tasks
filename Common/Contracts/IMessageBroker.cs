using Common.Contracts;

public interface IMessageBroker 
{
    Task SendMessage<T>(T message, CancellationToken cancellationToken = default) where T : IMessage;
}