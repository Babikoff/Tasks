using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TaskItemService.Data;
using TaskService.Models;

namespace TasksService.IntegrationTests;

public class TasksWebApplicationFactory : WebApplicationFactory<Program>
{
    public FakeMessageBroker MessageBroker { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove real PostgreSQL DbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<TasksDbContext>));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Add In-Memory DbContext
            services.AddDbContext<TasksDbContext>(options =>
            {
                options.UseInMemoryDatabase("InMemoryTasksTestDb");
            });

            // Remove real RabbitMQ MessageBroker registration
            var brokerDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IMessageBrocker));

            if (brokerDescriptor != null)
            {
                services.Remove(brokerDescriptor);
            }

            // Add our FakeMessageBroker
            services.AddSingleton<IMessageBrocker>(MessageBroker);
        });
    }
}

public class FakeMessageBroker : IMessageBrocker
{
    public List<TaskItemMessage> SentMessages { get; } = new();

    public bool SendTaskItemMessage(TaskItemMessage message)
    {
        SentMessages.Add(message);
        return true;
    }
}

