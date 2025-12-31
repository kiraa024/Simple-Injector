using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace SimpleInjector
{
    class Injector
    {
        [Flags]
        enum ProcAccess : uint
        {
            AllAccess = 0x001F0FFF
        }

        [Flags]
        enum MemType : uint
        {
            Commit = 0x1000,
            Reserve = 0x2000,
            Release = 0x8000
        }

        [Flags]
        enum MemProtect : uint
        {
            ExecuteReadWrite = 0x40
        }

        const uint WAIT_INFINITE = 0xFFFFFFFF;

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr OpenProcess(ProcAccess access, bool inherit, int pid);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr VirtualAllocEx(IntPtr process, IntPtr address, IntPtr size, MemType type, MemProtect protect);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool WriteProcessMemory(IntPtr process, IntPtr address, byte[] buffer, int size, out IntPtr written);

        [DllImport("kernel32.dll")]
        static extern IntPtr GetModuleHandle(string module);

        [DllImport("kernel32.dll")]
        static extern IntPtr GetProcAddress(IntPtr module, string procName);

        [DllImport("kernel32.dll")]
        static extern IntPtr CreateRemoteThread(IntPtr process, IntPtr threadAttr, uint stackSize, IntPtr startAddr, IntPtr param, uint flags, IntPtr threadId);

        [DllImport("kernel32.dll")]
        static extern uint WaitForSingleObject(IntPtr handle, uint millis);

        [DllImport("kernel32.dll")]
        static extern bool VirtualFreeEx(IntPtr process, IntPtr address, int size, MemType freeType);

        [DllImport("kernel32.dll")]
        static extern bool CloseHandle(IntPtr handle);

        static void Log(string message)
        {
            Console.WriteLine($"[Injector] {message}");
        }

        static void Main(string[] args)
        {
            Log("Starting injector...");

            if (args.Length != 2)
            {
                Log("Usage: SimpleInjector.exe <PID> <DLL Path>");
                return;
            }

            if (!int.TryParse(args[0], out int targetPid))
            {
                Log("Invalid PID specified.");
                return;
            }

            string dllFile = args[1];
            if (!File.Exists(dllFile))
            {
                Log($"DLL file not found: {dllFile}");
                return;
            }

            Log($"Target PID: {targetPid}");
            Log($"DLL Path: {dllFile}");

            byte[] dllBuffer = Encoding.ASCII.GetBytes(dllFile + "\0");

            IntPtr processHandle = OpenProcess(ProcAccess.AllAccess, false, targetPid);
            if (processHandle == IntPtr.Zero)
            {
                Log("Failed to open target process.");
                return;
            }
            Log("Successfully opened target process.");

            IntPtr remoteMem = VirtualAllocEx(processHandle, IntPtr.Zero, (IntPtr)dllBuffer.Length, MemType.Commit | MemType.Reserve, MemProtect.ExecuteReadWrite);
            if (remoteMem == IntPtr.Zero)
            {
                Log("Failed to allocate memory in target process.");
                CloseHandle(processHandle);
                return;
            }
            Log($"Allocated memory at 0x{remoteMem.ToInt64():X}");

            if (!WriteProcessMemory(processHandle, remoteMem, dllBuffer, dllBuffer.Length, out _))
            {
                Log("Failed to write DLL path into target process memory.");
                VirtualFreeEx(processHandle, remoteMem, 0, MemType.Release);
                CloseHandle(processHandle);
                return;
            }
            Log("DLL path written to target process memory.");

            IntPtr kernel32 = GetModuleHandle("kernel32.dll");
            IntPtr loadLibrary = GetProcAddress(kernel32, "LoadLibraryA");
            Log($"LoadLibraryA address: 0x{loadLibrary.ToInt64():X}");

            IntPtr threadHandle = CreateRemoteThread(processHandle, IntPtr.Zero, 0, loadLibrary, remoteMem, 0, IntPtr.Zero);
            if (threadHandle == IntPtr.Zero)
            {
                Log("Failed to create remote thread in target process.");
                VirtualFreeEx(processHandle, remoteMem, 0, MemType.Release);
                CloseHandle(processHandle);
                return;
            }
            Log("Remote thread created successfully, waiting for it to finish...");

            WaitForSingleObject(threadHandle, WAIT_INFINITE);

            VirtualFreeEx(processHandle, remoteMem, 0, MemType.Release);
            CloseHandle(threadHandle);
            CloseHandle(processHandle);

            Log("DLL injected successfully!");
        }
    }
}
