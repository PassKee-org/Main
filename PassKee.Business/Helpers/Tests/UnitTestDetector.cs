namespace PassKee.Business.Helpers.Tests;

public class UnitTestDetector
{
    private static bool _runningFromXUnit = false;      

    static UnitTestDetector()
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            // Can't do something like this as it will load the nUnit assembly
            // if (assem == typeof(NUnit.Framework.Assert))

            if (assembly.FullName?.ToLowerInvariant().StartsWith("xunit.runner") ?? false)
            {
                _runningFromXUnit = true;
                break;
            }
        }
    }

    public static bool IsRunningFromXUnit => _runningFromXUnit;
}
