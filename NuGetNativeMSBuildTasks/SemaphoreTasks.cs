using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoApp.NuGetNativeMSBuildTasks {
    using System.Runtime.InteropServices;
    using Microsoft.Build.Framework;

    internal static class SemaphoreFunctions {

        // NOT using the .Net Semaphore implementation (System.Threading.Semaphore) because it will automatically release the semaphore on object disposal.

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr CreateSemaphore(IntPtr lpSemaphoreAttributes, int initialCount, int maximumCount, string name);

        [DllImport("kernel32.dll")]
        public static extern bool ReleaseSemaphore(IntPtr hSemaphore, int lReleaseCount, IntPtr lpPreviousCount);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern UInt32 WaitForSingleObject(IntPtr hHandle, UInt32 dwMilliseconds);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CloseHandle(IntPtr hObject);

        public const UInt32 INFINITE = 0xFFFFFFFF;
        public const UInt32 WAIT_OBJECT_0 = 0x00000000;
        public const UInt32 ERROR_ALREADY_EXISTS = 183;

        public static IntPtr ParseHandle(string value) {
            if (IntPtr.Size == 4)
                return new IntPtr(int.Parse(value));
            if (IntPtr.Size == 8)
                return new IntPtr(long.Parse(value));
            throw new Exception("What kind of pointer is this?!");
        }
    }

    public class AcquireSemaphore : MsBuildTaskBase {

        private IntPtr _handle;

        [Required]
        public string Name {get; set;}

        [Output]
        public string Handle {
            get {
                return _handle.ToString();
            }
            set {
                _handle = SemaphoreFunctions.ParseHandle(value);
            }
        }

        public override bool Execute() {
            _handle = SemaphoreFunctions.CreateSemaphore(IntPtr.Zero, 0, 1, Name);
            if (_handle == IntPtr.Zero)
                throw new Exception("CreateSemaphore failed with error #" + Marshal.GetLastWin32Error());

            bool createdNew = (Marshal.GetLastWin32Error() != SemaphoreFunctions.ERROR_ALREADY_EXISTS);

            if (!createdNew) {
                uint retval = SemaphoreFunctions.WaitForSingleObject(_handle, SemaphoreFunctions.INFINITE);
                if (retval != SemaphoreFunctions.WAIT_OBJECT_0) {
                    SemaphoreFunctions.CloseHandle(_handle);
                    throw new Exception("WaitForSingleObject failed with error #" + Marshal.GetLastWin32Error());
                }
            }

            return true;
        }
    }

    public class ReleaseSemaphore : MsBuildTaskBase {

        private IntPtr _handle;

        [Required]
        public string Handle {
            get {
                return _handle.ToString();
            }
            set {
                _handle = SemaphoreFunctions.ParseHandle(value);
            }
        }

        public override bool Execute() {
            try {
                bool retval = SemaphoreFunctions.ReleaseSemaphore(_handle, 1, IntPtr.Zero);
                if (!retval)
                    throw new Exception("ReleaseSemaphore failed with error #" + Marshal.GetLastWin32Error());
                return true;
            } finally {
                SemaphoreFunctions.CloseHandle(_handle);
            }
        }
    }
}