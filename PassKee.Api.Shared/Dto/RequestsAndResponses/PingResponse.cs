using Api.Requests.Abstractions;

namespace PassKee.Api.Shared.Dto.RequestsAndResponses;

public class PingResponse : IResponse
{
    public string Message { get; set; } = "Pong";
}

