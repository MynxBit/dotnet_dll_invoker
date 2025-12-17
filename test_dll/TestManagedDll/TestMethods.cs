using System;
using System.Diagnostics;
using System.Threading;

namespace TestManagedDll
{
    public class TestMethods
    {
        // Helper to visualize execution
        private static void LaunchCmd(string message)
        {
            Console.WriteLine($"[TestDLL:Log] Launching CMD with message: {message}");
            Process.Start(new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c echo [POPUP] {message} & timeout 3",
                UseShellExecute = true,
                CreateNoWindow = false
            });
        }

        // Method 1: String Parameter
        public void Method1(string input)
        {
            Console.WriteLine($"[TestDLL:Stdout] Executing Method1 with input: '{input}'");
            LaunchCmd($"Method1 says: {input}");
        }

        // Method 2: Int Parameter
        public void Method2(int number)
        {
            Console.WriteLine($"[TestDLL:Stdout] Executing Method2 with number: {number}");
            LaunchCmd($"Method2 received: {number}");
        }

        // Method 3: Complex Parameters
        public void Method3(double value, string note)
        {
            Console.WriteLine($"[TestDLL:Stdout] Executing Method3 with {value} and '{note}'");
            LaunchCmd($"Method3: {value} - {note}");
        }

        // Method 4: Boolean Logic
        public void Method4(bool flag)
        {
            Console.WriteLine($"[TestDLL:Stdout] Executing Method4 with flag: {flag}");
            if (flag)
            {
                LaunchCmd("Method4: ACTION TAKEN");
            }
            else
            {
                Console.Error.WriteLine("[TestDLL:Stderr] Make sure to set flag=true to see popup!");
            }
        }

        // Method 5: Conditional Logic
        public void Method5(int special)
        {
            Console.WriteLine($"[TestDLL:Stdout] Executing Method5 checking for secret 5. Got: {special}");
            if (special == 5)
                LaunchCmd("Method5: SECRET UNLOCKED");
            else
                Console.WriteLine("[TestDLL:Stdout] Wrong number. Try 5.");
        }

        // Method 6: Exception Test
        public void Method6_Crash()
        {
            Console.WriteLine("[TestDLL:Stdout] preparing to crash...");
            throw new InvalidOperationException("This is a simulated crash from Method6!");
        }

        // Method 7: Static Method (No Instance)
        public static void Method7_Static()
        {
             Console.WriteLine("[TestDLL:Stdout] Static method invoked!");
             LaunchCmd("Method7: I AM STATIC");
        }
        
        // Method 8: Long Running (Hang simulation)
        public void Method8_Slow()
        {
            Console.WriteLine("[TestDLL:Stdout] Sleeping for 4 seconds...");
            Thread.Sleep(4000);
            Console.WriteLine("[TestDLL:Stdout] Woke up!");
        }
    }
}
