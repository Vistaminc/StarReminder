using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using MediaDetectionSystem.Models;

namespace MediaDetectionSystem.Services
{
    public class ProcessController
    {
        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenThread(ThreadAccess dwDesiredAccess, bool bInheritHandle, uint dwThreadId);

        [DllImport("kernel32.dll")]
        private static extern uint SuspendThread(IntPtr hThread);

        [DllImport("kernel32.dll")]
        private static extern int ResumeThread(IntPtr hThread);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hObject);

        [Flags]
        private enum ThreadAccess : int
        {
            SUSPEND_RESUME = 0x0002
        }

        public bool SuspendProcess(int processId)
        {
            try
            {
                var process = Process.GetProcessById(processId);
                
                foreach (ProcessThread thread in process.Threads)
                {
                    IntPtr pOpenThread = OpenThread(ThreadAccess.SUSPEND_RESUME, false, (uint)thread.Id);
                    
                    if (pOpenThread == IntPtr.Zero)
                        continue;

                    SuspendThread(pOpenThread);
                    CloseHandle(pOpenThread);
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"挂起进程失败: {ex.Message}");
                return false;
            }
        }

        public bool ResumeProcess(int processId)
        {
            try
            {
                var process = Process.GetProcessById(processId);
                
                foreach (ProcessThread thread in process.Threads)
                {
                    IntPtr pOpenThread = OpenThread(ThreadAccess.SUSPEND_RESUME, false, (uint)thread.Id);
                    
                    if (pOpenThread == IntPtr.Zero)
                        continue;

                    ResumeThread(pOpenThread);
                    CloseHandle(pOpenThread);
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"恢复进程失败: {ex.Message}");
                return false;
            }
        }

        public bool TerminateProcess(int processId)
        {
            try
            {
                var process = Process.GetProcessById(processId);
                process.Kill();
                process.WaitForExit(5000);
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"终止进程失败: {ex.Message}");
                return false;
            }
        }

        public bool IsProcessRunning(int processId)
        {
            try
            {
                var process = Process.GetProcessById(processId);
                return !process.HasExited;
            }
            catch
            {
                return false;
            }
        }
    }
}
