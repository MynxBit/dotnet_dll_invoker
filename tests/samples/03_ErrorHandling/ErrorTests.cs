// Error Handling Test DLL
// Tests exception handling and error scenarios

namespace Sample03_ErrorHandling;

/// <summary>
/// Tests various exception types.
/// </summary>
public class ExceptionThrower
{
    // Standard .NET exceptions
    public void ThrowInvalidOperation()
    {
        throw new InvalidOperationException("This operation is not valid!");
    }
    
    public void ThrowArgumentNull(string? name)
    {
        ArgumentNullException.ThrowIfNull(name);
    }
    
    public void ThrowArgumentOutOfRange(int value)
    {
        if (value < 0 || value > 100)
            throw new ArgumentOutOfRangeException(nameof(value), "Must be 0-100");
    }
    
    public void ThrowCustomException()
    {
        throw new CustomBusinessException("Business rule violated", 1001);
    }
    
    // Exception with inner exception
    public void ThrowWithInner()
    {
        try
        {
            throw new InvalidDataException("Data is corrupt");
        }
        catch (Exception inner)
        {
            throw new ApplicationException("Failed to process data", inner);
        }
    }
}

/// <summary>
/// Tests timeout and hang scenarios.
/// </summary>
public class TimeoutTests
{
    public void SlowMethod()
    {
        Thread.Sleep(5000); // 5 second sleep
    }
    
    // DANGER: This will hang forever!
    public void InfiniteLoop()
    {
        while (true) 
        { 
            Thread.Sleep(100); 
        }
    }
    
    public void DeadlockSimulation()
    {
        var lockA = new object();
        var lockB = new object();
        
        // This won't actually deadlock in single thread,
        // but demonstrates the concept
        lock (lockA)
        {
            lock (lockB)
            {
                Thread.Sleep(1000);
            }
        }
    }
}

/// <summary>
/// Custom exception for testing.
/// </summary>
public class CustomBusinessException : Exception
{
    public int ErrorCode { get; }
    
    public CustomBusinessException(string message, int errorCode) 
        : base(message)
    {
        ErrorCode = errorCode;
    }
}
