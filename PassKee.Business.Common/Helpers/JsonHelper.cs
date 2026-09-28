using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace PassKee.Business.Common.Helpers;

public static class JsonHelper
{
    public static string SerializeToString(object data, DateTimeZoneHandling? dateTimeZoneHandling = null)
    {
        return JsonConvert.SerializeObject(data, Formatting.Indented, new JsonSerializerSettings()
        {
            DateTimeZoneHandling = dateTimeZoneHandling ?? DateTimeZoneHandling.Utc
        });
    }
    
    public static byte[]? SerializeToBytes(object data)
    {
        var jsonString = SerializeToString(data);
        if (!string.IsNullOrEmpty(jsonString))
        {
            return Encoding.UTF8.GetBytes(jsonString);
        }

        return null;
    }
    
    public static T? DeserializeObject<T>(
        string value,
        DateTimeZoneHandling? dateTimeZoneHandling = null,
        List<JsonConverter>? converters = null,
        IContractResolver? contractResolver = null
    )
    {
        var settings = new JsonSerializerSettings()
        {
            DateTimeZoneHandling = dateTimeZoneHandling ?? DateTimeZoneHandling.Utc
        };
        if (converters != null)
        {
            settings.Converters = converters;
        }
        if (contractResolver != null)
        {
            settings.ContractResolver = contractResolver;
        }
        return JsonConvert.DeserializeObject<T>(value, settings);
    }

    public static T? DeserializeObject<T>(
        byte[] bytes,
        DateTimeZoneHandling? dateTimeZoneHandling = null,
        List<JsonConverter>? converters = null,
        IContractResolver? contractResolver = null
    )
    {
        if (bytes == null || bytes.Length == 0)
        {
            return default;
        }

        var jsonString = Encoding.UTF8.GetString(bytes);
        return DeserializeObject<T>(jsonString, dateTimeZoneHandling, converters, contractResolver);
    }
    
    public static object? DeserializeObject(
        string value,
        Type type,
        DateTimeZoneHandling? dateTimeZoneHandling = null,
        List<JsonConverter>? converters = null,
        IContractResolver? contractResolver = null
    )
    {
        var settings = new JsonSerializerSettings()
        {
            DateTimeZoneHandling = dateTimeZoneHandling ?? DateTimeZoneHandling.Utc
        };
        if (converters != null)
        {
            settings.Converters = converters;
        }
        if (contractResolver != null)
        {
            settings.ContractResolver = contractResolver;
        }
        return JsonConvert.DeserializeObject(value, type, settings);
    }
}
