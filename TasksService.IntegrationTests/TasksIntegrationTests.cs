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

        // -- 4. Act: check GET by provided URL
        var getCreatedTaskResponse = await _client.GetAsync(createResult.Url);
        Assert.Equal(HttpStatusCode.OK, getCreatedTaskResponse.StatusCode);
        var retrievedCreatedTask = await getCreatedTaskResponse.Content.ReadFromJsonAsync<TaskItemDto>();
        Assert.NotNull(retrievedCreatedTask);
        Assert.Equal(createResult.Id, retrievedCreatedTask.Id);

        // --- 5. Act: Complete Task ---
        var completeResponse = await _client.PutAsync($"/api/tasks/{createResult.Id}/complete", null);

        // --- 6. Assert: Completion success ---
        Assert.Equal(HttpStatusCode.OK, completeResponse.StatusCode);

        var isCompletedSuccess = await completeResponse.Content.ReadFromJsonAsync<bool>();
        Assert.True(isCompletedSuccess);

        // --- 7. Assert: Broker / State change ---
        // Verify that the integration test database was updated (via GET endpoint)
        var getCompletedTaskResponse = await _client.GetAsync($"/api/tasks/{createResult.Id}");
        Assert.Equal(HttpStatusCode.OK, getCompletedTaskResponse.StatusCode);

        var retrievedCompletedTask = await getCompletedTaskResponse.Content.ReadFromJsonAsync<TaskItemDto>();
        Assert.NotNull(retrievedCompletedTask);
        Assert.True(retrievedCompletedTask.IsCompleted);
        Assert.Equal(createResult.Id, retrievedCompletedTask.Id);

        // Verify that a message was sent to the broker
        var sentMessage = _factory.MessageBroker.SentMessages.FirstOrDefault(m => m.Id == createResult.Id);
        Assert.NotNull(sentMessage);
        Assert.Equal(createResult.Id, sentMessage.Id);
        var taskMessage = sentMessage as TaskItemMessage;
        Assert.NotNull(taskMessage);
        Assert.Equal(taskTitle, taskMessage.Title);
        Assert.True(taskMessage.IsCompleted);
    }
}

// Simple record or class to map response for verification
public record TaskItemDto(Guid Id, string Title, bool IsCompleted, Priority Priority, DateTimeOffset CreatedAt, DateTime? CompletedAt);

