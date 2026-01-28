using System.Net;
using System.Text;

namespace FindThatBook.Tests.Common.Mocks;

public class MockHttpMessageHandler : HttpMessageHandler
{
    private readonly Queue<HttpResponseMessage> _responses = new();
    private readonly List<HttpRequestMessage> _requests = new();

    public IReadOnlyList<HttpRequestMessage> ReceivedRequests => _requests;

    public MockHttpMessageHandler QueueResponse(HttpStatusCode statusCode, string content, string contentType = "application/json")
    {
        _responses.Enqueue(new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(content, Encoding.UTF8, contentType)
        });
        return this;
    }

    public MockHttpMessageHandler QueueSuccessResponse(string jsonContent)
    {
        return QueueResponse(HttpStatusCode.OK, jsonContent);
    }

    public MockHttpMessageHandler QueueErrorResponse(HttpStatusCode statusCode, string errorMessage)
    {
        return QueueResponse(statusCode, $"{{\"error\": \"{errorMessage}\"}}");
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        _requests.Add(request);

        if (_responses.Count == 0)
        {
            throw new InvalidOperationException("No responses queued. Call QueueResponse before making requests.");
        }

        return Task.FromResult(_responses.Dequeue());
    }
}
