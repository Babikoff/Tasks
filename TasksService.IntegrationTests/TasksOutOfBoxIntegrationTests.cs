using System.Net;
using System.Net.Http.Json;
using TaskService.Common;
using TaskService.Models;

namespace TasksService.IntegrationTests;

[Trait("Category", "Integration.BlackBox")]
public class TasksOutOfBoxIntegrationTests
{
    private const string BaseUrl = "http://localhost:8080";
    private readonly HttpClient _client;
    private readonly RabbitMqHttpHelper _rabbitMqHelper;

    public TasksOutOfBoxIntegrationTests()
    {
        _client = new HttpClient
        {
            BaseAddress = new Uri(BaseUrl),
            Timeout = TimeSpan.FromSeconds(5)
        };

        _rabbitMqHelper = new RabbitMqHttpHelper();
    }

    private async Task EnsureServiceIsRunningAsync()
    {
        try
        {
            var healthResponse = await _client.GetAsync("/health");
            if (!healthResponse.IsSuccessStatusCode)
            {
                Assert.Fail($"Tasks Service at {BaseUrl} responded with status code {healthResponse.StatusCode} on /health. Ensure Docker containers are running healthy.");
            }
        }
        catch (Exception ex)
        {
            Assert.Fail($"Could not connect to Tasks Service at {BaseUrl}. Ensure Docker containers are up (e.g. 'docker compose up -d'). Error: {ex.Message}");
        }
    }

    [Fact]
    public async Task CreateAndCompleteTask_AgainstLiveContainer_ShouldSucceed()
    {
        // --- 0. Pre-check: Ensure containerized service is reachable ---
        await EnsureServiceIsRunningAsync();

        // --- 1. Arrange ---
        var taskTitle = $"BlackBox_Task_{Guid.NewGuid()}";
        var createRequest = new CreateTaskRequest(
            Title: taskTitle,
            IsCompleted: false,
            Priority: Priority.Low
        );

        // --- 2. Act: Create Task ---
        var createResponse = await _client.PostAsJsonAsync("/api/tasks", createRequest);

        // --- 3. Assert: Creation status & body ---
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var createResult = await createResponse.Content.ReadFromJsonAsync<CreateTaskResponse>();
        Assert.NotNull(createResult);
        Assert.NotEqual(Guid.Empty, createResult.Id);
        Assert.Equal(taskTitle, createResult.Title);
        Assert.False(createResult.IsCompleted);

        // --- 4. Act: Complete Task ---
        var completeResponse = await _client.PutAsync($"/api/tasks/{createResult.Id}/complete", null);

        // --- 5. Assert: Complete endpoint response ---
        Assert.Equal(HttpStatusCode.OK, completeResponse.StatusCode);
        var completeResult = await completeResponse.Content.ReadFromJsonAsync<bool>();
        Assert.True(completeResult);

        // --- 6. Assert: Verify via GET endpoint that task is updated ---
        var getResponse = await _client.GetAsync($"/api/tasks/{createResult.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var retrievedTask = await getResponse.Content.ReadFromJsonAsync<TaskItemDto>();
        Assert.NotNull(retrievedTask);
        Assert.Equal(createResult.Id, retrievedTask.Id);
        Assert.True(retrievedTask.IsCompleted);

        // --- 7. Assert: RabbitMQ Message Verification via Management API ---
        var isMessageInQueue = await _rabbitMqHelper.WaitForMessageWithTaskIdAsync(
            createResult.Id.ToString(),
            timeout: TimeSpan.FromSeconds(5)
        );
        Assert.True(isMessageInQueue, $"Expected task completion message with taskId '{createResult.Id}' in RabbitMQ queue, but it was not found.");
    }
}

