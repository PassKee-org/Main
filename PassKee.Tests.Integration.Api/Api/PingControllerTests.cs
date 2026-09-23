using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using PassKee.Api.Shared.Constants;
using PassKee.Api.Shared.Dto.RequestsAndResponses;
using PassKee.Tests.Integration.Api.Core;
using Xunit;

namespace PassKee.Tests.Integration.Api.Api;

public class PingControllerTests : BaseTest
{
    public PingControllerTests(ApiCustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Ping_ShouldReturnPong()
    {
        var response = await GetRequestAsAnonymousAsync(ApiUrl.Ping);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var content = await response.Content.ReadFromJsonAsync<PingResponse>();
        Assert.NotNull(content);
        Assert.Equal("Pong", content!.Message);
    }
}
