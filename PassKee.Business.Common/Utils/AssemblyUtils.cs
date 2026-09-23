using System.Reflection;

namespace PassKee.Business.Common.Utils
{
    public static class AssemblyUtils
    {
        public static string GetAssemblyPath(Assembly assembly = null)
        {
            assembly ??= Assembly.GetExecutingAssembly();
            string codeBase = assembly.CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);
            string path = Uri.UnescapeDataString(uri.Path);
            return Path.GetDirectoryName(path);
        }
    }
}
