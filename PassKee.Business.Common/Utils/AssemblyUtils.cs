using System.Reflection;

namespace PassKee.Business.Common.Utils
{
    public static class AssemblyUtils
    {
        public static string GetAssemblyPath(Assembly? assembly = null)
        {
            assembly ??= Assembly.GetExecutingAssembly();
            return Path.GetDirectoryName(assembly.Location) ?? AppContext.BaseDirectory;
        }
    }
}
