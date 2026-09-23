using System.Net;
using Newtonsoft.Json;
using PassKee.Business.Common.Dto;
using PassKee.Business.Common.Helpers;

namespace PassKee.Business.Common.Extensions
{
    public static class HttpResponseExtensions
    {
        public static async Task<T> GetJsonDataAsync<T>(this HttpResponseMessage response)
        {
            if (response.StatusCode == HttpStatusCode.NotFound)
                throw new Exception($"Got response: {HttpStatusCode.NotFound}");
            var stringData = await response.GetDataAsStringAsync();
            return JsonHelper.DeserializeObject<T>(stringData, DateTimeZoneHandling.Utc)!;
        }
        
        public static async Task<object?> GetJsonDataAsync(this HttpResponseMessage response)
        {
            return await response.GetJsonDataAsync<object>();
        }

        public static async Task<string> GetDataAsStringAsync(this HttpResponseMessage response)
        {
            return await response.Content.ReadAsStringAsync();
        }
        
        public static async Task EnsureSuccessStatusCodeWithoutError(this HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                var responseModel = await response.GetJsonResponseAsync<object>();
                throw new HttpRequestException($"Http request response returned an error: {responseModel.ErrorCode} - {responseModel.Message}");
            }
        }
        
        public static async Task<JsonCommonResponse<T>> GetJsonResponseAsync<T>(this HttpResponseMessage response)
        {
            return (await response.GetJsonDataAsync<JsonCommonResponse<T>>())!;
        }
        
        public static async Task<T> GetJsonResponseDataAsync<T>(this HttpResponseMessage response)
        {
            var jsonResponse = await response.GetJsonResponseAsync<T>();
            if (jsonResponse == null)
            {
                throw new Exception("Model was not parsed");
            }
            return jsonResponse.Data;
        }
    }
}
