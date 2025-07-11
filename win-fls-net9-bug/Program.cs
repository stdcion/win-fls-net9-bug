using System.Runtime.InteropServices;

namespace win_fls_net9_bug;

internal static class Program
{
    private static readonly ThreadExitCallback OnThreadExit = DoSomethingOnThreadExit;
    private static readonly int ThreadExitCallbackId = RegisterCallbackDelegate(OnThreadExit);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void ThreadExitCallback(nint threadLocalValue);

    private static void Main(string[] args)
    {
        Console.WriteLine($".NET Version: {RuntimeInformation.FrameworkDescription}");

        var thread = new Thread(() =>
        {
            Console.WriteLine("Thread started");
            var handle = GCHandle.Alloc(42, GCHandleType.Pinned);
            // Register callback to be invoked when current thread exits.
            EnableThreadExitCallbackForCurrentThread(ThreadExitCallbackId, GCHandle.ToIntPtr(handle));
        }) { IsBackground = false };
        thread.Start();
        thread.Join(); // Wait for the thread to complete and callback to execute.
    }

    private static void DoSomethingOnThreadExit(nint threadLocalValue)
    {
        // The content of the method doesn't matter,
        // I can register an empty method, or a reference to a native method, the result is the same:
        // Attempt to execute managed code after the .NET runtime thread state has been destroyed.
        var handle = GCHandle.FromIntPtr(threadLocalValue);
        Console.WriteLine("Thread exit, thread local value: " + handle.Target);
        handle.Free();
    }

    private static int RegisterCallbackDelegate(ThreadExitCallback callback)
    {
        var callbackPtr = Marshal.GetFunctionPointerForDelegate(callback);
        var callbackId = WindowsImport.FlsAlloc(callbackPtr);

        if (callbackId == WindowsImport.FlsOutOfIndexes)
        {
            throw new InvalidOperationException("FlsAlloc failed: " + Marshal.GetLastWin32Error());
        }

        return callbackId;
    }

    private static void EnableThreadExitCallbackForCurrentThread(int callbackId, nint threadLocalValue)
    {
        var result = WindowsImport.FlsSetValue(callbackId, threadLocalValue);

        if (!result)
        {
            throw new InvalidOperationException("FlsSetValue failed: " + Marshal.GetLastWin32Error());
        }
    }

    /// <summary>
    /// P/Invoke declarations for Windows FLS (Fiber Local Storage) APIs.
    /// FLS provides thread-local storage with destructor callbacks.
    /// </summary>
    private static class WindowsImport
    {
        public const string DllName = "kernel32.dll";

        public const int FlsOutOfIndexes = -1;

        [DllImport(DllName, SetLastError = true)]
        public static extern int FlsAlloc(nint destructorCallback);

        [DllImport(DllName, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool FlsSetValue(int key, nint threadLocalValue);

    }
}
