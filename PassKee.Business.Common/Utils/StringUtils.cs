namespace PassKee.Business.Common.Utils;

public static class StringUtils
{
    public static string? GetUserNameFromEmail(string email)
    {
        if (string.IsNullOrEmpty(email))
        {
            return null;
        }
        var paths = email.Split("@");
        if (paths.Length <= 1)
        {
            return null;
        }
        return paths.First().ToLower();
    }
    
    public static string BytesToString(long byteCount)
    {
        string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" }; //Longs run out around EB
        if (byteCount == 0)
            return "0" + suf[0];
        long bytes = Math.Abs(byteCount);
        int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
        double num = Math.Round(bytes / Math.Pow(1024, place), 1);
        return (Math.Sign(byteCount) * num).ToString() + suf[place];
    }
}
