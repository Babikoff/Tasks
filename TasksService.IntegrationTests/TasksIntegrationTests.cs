using System.Net;
using System.Net.Http.Json;
using TaskService.Common;
using TaskService.Models;

namespace TasksService.IntegrationTests;

public class TasksIntegrationTests : IClassFixture<TasksWebApplicationFactory>
{
    private readonly TasksWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public TasksIntegrationTests(TasksWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task CreateAndCompleteTask_ShouldSucceed()
    {
        // --- 1. Arrange ---
        var taskTitle = $"Integration_Test_Task_{Guid.NewGuid()}";
        var createRequest = new CreateTaskRequest(
            Title: taskTitle,
            IsCompleted: false,
            Priority: Priority.Low
        );

        // --- 2. Act: Create Task ---
        var createResponse = await _client.PostAsJsonAsync("/api/tasks", createRequest);

        // --- 3. Assert: Creation success ---
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        
        var createResult = await createResponse.Content.ReadFromJsonAsync<CreateTaskResponse>();
        Assert.NotNull(createResult);
        Assert.NotEqual(Guid.Empty, createResult.Id);
        Assert.Equal(taskTitle, createResult.Title);
        Assert.False(createResult.IsCompleted);

        // --- 4. Act: Complete Task ---
        var completeResponse = await _client.PutAsync($"/api/tasks/{createResult.Id}/complete", null);

        // --- 5. Assert: Completion success ---
        Assert.Equal(HttpStatusCode.OK, completeResponse.StatusCode);

        var isCompletedSuccess = await completeResponse.Content.ReadFromJsonAsync<bool>();
        Assert.True(isCompletedSuccess);

        // --- 6. Assert: Broker / State change ---
        // Verify that the integration test database was updated (via GET endpoint)
        var getResponse = await _client.GetAsync($"/api/tasks/{createResult.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        
        var retrievedTask = await getResponse.Content.ReadFromJsonAsync<TaskItemDto>();
        Assert.NotNull(retrievedTask);
        Assert.True(retrievedTask.IsCompleted);

        // Verify that a message was sent to the broker
        var sentMessage = _factory.MessageBroker.SentMessages.FirstOrDefault(m => m.taskId == createResult.Id.ToString());
        Assert.NotNull(sentMessage);
        Assert.Equal(createResult.Id.ToString(), sentMessage.taskId);
        Assert.Equal(taskTitle, sentMessage.title);
        Assert.True(sentMessage.isCompleted);
    }
}

// Simple record or class to map response for verification
public record TaskItemDto(Guid Id, string Title, bool IsCompleted, Priority Priority, DateTimeOffset CreatedAt, DateTime? CompletedAt);

