using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PassKee.Business.Common.Constants;

namespace PassKee.Business.Common.Dto;

public class JsonCommonResponse
{
    [JsonConverter(typeof(StringEnumConverter))]
    [JsonProperty(PropertyName = "status")]
    public HttpResponseStatus Status { get; set; } = HttpResponseStatus.Fail;
        
    [JsonProperty(PropertyName = "message")]
    public string Message { get; set; } = string.Empty;

    [JsonProperty(PropertyName = "errorCode")]
    public string ErrorCode { get; set; } = string.Empty;

    [JsonProperty(PropertyName = "data")]
    public object Data { get; set; } = new { };
}
    
public class JsonCommonResponse<T>: JsonCommonResponse
{
    [JsonProperty(PropertyName = "data")]
    public new required T Data { get; set; }
}
