using MessageBrokers.Common.Contracts;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TaskItemService.Data;

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
                d => d.ServiceType == typeof(IMessageBroker));

            if (brokerDescriptor != null)
            {
                services.Remove(brokerDescriptor);
            }

            // Add our FakeMessageBroker
            services.AddSingleton<IMessageBroker>(MessageBroker);
        });
    }
}

public class FakeMessageBroker : IMessageBroker
{
    public List<IMessage> SentMessages { get; } = new();

    public async Task SendMessage<T>(T message, CancellationToken cancellationToken = default) where T : IMessage
    {
        SentMessages.Add(message);
    }
}

