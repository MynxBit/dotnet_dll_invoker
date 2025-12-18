namespace TestManagedDll_x86;

public class LegacyService
{
    public static string GetArchitecture()
    {
        return System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture.ToString();
    }

    public int Add(int a, int b) => a + b;
}
