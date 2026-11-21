using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace TasksService.IntegrationTests;

public class RabbitMqHttpHelper
{
    private readonly HttpClient _client;
    private readonly string _queueName;

    public RabbitMqHttpHelper(
        string baseUrl = "http://localhost:15672",
        string username = "rabbitmq",
        string password = "rabbitmq",
        string queueName = "tasks-queue")
    {
        _queueName = Uri.EscapeDataString(queueName);
        _client = new HttpClient
        {
            BaseAddress = new Uri(baseUrl),
            Timeout = TimeSpan.FromSeconds(5)
        };

        var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
    }

    /// <summary>
    /// Checks if a message containing the expected taskId exists in the queue by polling the RabbitMQ Management API.
    /// Uses ackmode: ack_requeue_true to peek at messages without consuming them.
    /// </summary>
    public async Task<bool> WaitForMessageWithTaskIdAsync(string taskId, TimeSpan timeout)
    {
        var startTime = DateTime.UtcNow;

        while (DateTime.UtcNow - startTime < timeout)
        {
            try
            {
                // Request up to 10 messages from the default vhost ('%2F')
                var requestBody = new
                {
                    count = 10,
                    ackmode = "ack_requeue_true",
                    encoding = "auto",
                    truncate = 50000
                };

                var response = await _client.PostAsJsonAsync($"/api/queues/%2F/{_queueName}/get", requestBody);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    if (!string.IsNullOrWhiteSpace(content) && content.Contains(taskId))
                    {
                        return true;
                    }
                }
            }
            catch
            {
                // Ignore transient network errors during polling
            }

            await Task.Delay(500);
        }

        return false;
    }
}
