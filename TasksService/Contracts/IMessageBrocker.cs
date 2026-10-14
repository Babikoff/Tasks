using TaskService.Models;

public interface IMessageBrocker
{
  bool SendTaskItemMessage(TaskItemMessage msg);

}