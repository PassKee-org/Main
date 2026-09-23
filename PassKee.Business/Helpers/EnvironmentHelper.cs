namespace PassKee.Business.Helpers;

public static class EnvironmentHelper
{
    public static string GetHostName()
    {
        return Environment.GetEnvironmentVariable("HOSTNAME");
    }
    
    public static string GetPodId()
    {
        var hostName = $"{GetHostName()}";
        var hostNameParts = hostName.Split("-");
        return hostNameParts.LastOrDefault();
    }
}
