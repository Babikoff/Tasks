namespace MessageBrokers.Common.Contracts
{
    public interface IMessage
    {
        Guid Id { get; init; }
    }
}
