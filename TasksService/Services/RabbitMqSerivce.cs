using TaskService.Models;

public class RabbitMqSerivce : IMessageBrocker
{
  public bool SendTaskItemMessage(TaskItemMessage msg)
  {
    return true; //TODO: impletent
  }
}