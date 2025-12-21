namespace TestManagedDll_x64;

public class X64Service
{
    public static string GetArchitecture()
    {
        return System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture.ToString();
    }

    public int Multiply(int a, int b) => a * b;
}
