using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace System.Diagnostics
{
    public sealed class Memory
    {
        public MemoryPermissions Permissions { get; private set; }

        Process Proc;

        /// <summary>
        /// Returns the process descriptor
        /// </summary>
        public IntPtr Handle { get; private set; }
        /// <summary>
        /// Returns the name of the process
        /// </summary>
        public string ProcessName
        {
            get
            {
                return Proc.ProcessName;
            }
        }
        /// <summary>
        /// Returns the status of the process
        /// </summary>
        public bool Status
        {
            get
            {
                return Proc.Responding;
            }
        }
        /// <summary>
        /// Returns the memory occupied by the process
        /// </summary>
        public double MemoryUsage
        {
            get
            {
                return new PerformanceCounter("Process", "Working Set", ProcessName, true).NextValue();
            }
        }
        /// <summary>
        /// Returns the CPU usage value of the process
        /// </summary>
        public double CpuUsage
        {
            get
            {
                return new PerformanceCounter("Process", "% Processor Time", ProcessName, true).NextValue();
            }
        }



        public Memory(string processname, string modulename, MemoryPermissions permissions = MemoryPermissions.All)
        {
            Permissions = permissions;
            Proc = Process.GetProcessesByName(processname).First();
            Handle = OpenProcess(2035711, false, this.Proc.Id);
        }

        public Memory(Process process, string modulename, MemoryPermissions permissions = MemoryPermissions.All)
        {
            Permissions = permissions;
            Proc = process;
            Handle = OpenProcess(2035711, false, this.Proc.Id);
        }

        public Memory(Process[] processes, string modulename, MemoryPermissions permissions = MemoryPermissions.All)
        {
            Permissions = permissions;
            Proc = processes.First();
            Handle = OpenProcess(2035711, false, this.Proc.Id);
        }

        /// <summary>
        /// Finds a pattern
        /// </summary>
        public IntPtr FindPattern(string modulename, string pattern, int offset = 0)
        {
            return this.FindPattern(Proc.GetModule(modulename), this.GetPattern(pattern), offset);
        }
        /// <summary>
        /// Finds a pattern
        /// </summary>
        public IntPtr FindPattern(ProcessModule module, byte[] pattern, int offset = 0)
        {
            bool flag = module == null;
            if (flag)
            {
                throw new ArgumentNullException("Mod");
            }
            return this.FindPattern(module.BaseAddress, (IntPtr)module.ModuleMemorySize, pattern, offset);
        }
        /// <summary>
        /// Finds a pattern
        /// </summary>
        public IntPtr FindPattern(ProcessModule module, string pattern, int offset = 0)
        {
            return this.FindPattern(module, this.GetPattern(pattern), offset);
        }
        /// <summary>
        /// Finds a pattern
        /// </summary>
        public IntPtr FindPattern(IntPtr startAddress, IntPtr endAddress, byte[] pattern, int offset)
        {
            if (Permissions != MemoryPermissions.All || Permissions != MemoryPermissions.Readonly) throw new Exception("Memory access denied: permission does not match the method being called");
            bool flag = pattern == null;
            if (flag)
            {
                throw new ArgumentNullException("pattern");
            }
            long num = startAddress.ToInt64() + endAddress.ToInt64();
            for (long num2 = startAddress.ToInt64(); num2 < num; num2 += 1L)
            {
                bool flag2 = true;
                for (int i = 0; i < pattern.Length; i++)
                {
                    flag2 = (pattern[i] == 0 || this.Read<byte>((IntPtr)(num2 + (long)i)) == pattern[i]);
                    bool flag3 = !flag2;
                    if (flag3)
                    {
                        break;
                    }
                }
                bool flag4 = flag2;
                if (flag4)
                {
                    return (IntPtr)(num2 + (long)offset);
                }
            }
            return IntPtr.Zero;
        }
        /// <summary>
        /// Reads memory
        /// </summary>
        /// <typeparam name="T">The type of data to get from memory</typeparam>
        /// <returns>Value</returns>
        public T Read<T>(int address)
        {
            if (Permissions != MemoryPermissions.All || Permissions != MemoryPermissions.Readonly) throw new Exception("Memory access denied: permission does not match the method being called");
            byte[] array = new byte[Marshal.SizeOf<T>()];
            ReadProcessMemory(this.Handle, (IntPtr)address, array, Marshal.SizeOf<T>(), 0);
            return this.GetStruct<T>(array);
        }
        /// <summary>
        /// Reads memory
        /// </summary>
        /// <typeparam name="T">The type of data to get from memory</typeparam>
        /// <returns>Value</returns>
        public T Read<T>(IntPtr address)
        {
            if (Permissions != MemoryPermissions.All || Permissions != MemoryPermissions.Readonly) throw new Exception("Memory access denied: permission does not match the method being called");
            byte[] array = new byte[Marshal.SizeOf<T>()];
            ReadProcessMemory(this.Handle, address, array, Marshal.SizeOf<T>(), 0);
            return this.GetStruct<T>(array);
        }
        /// <summary>
        /// Reads a string from memory
        /// </summary>
        public string ReadString(int address, int bufferSize, Encoding enc)
        {
            if (Permissions != MemoryPermissions.All || Permissions != MemoryPermissions.Readonly) throw new Exception("Memory access denied: permission does not match the method being called");
            byte[] array = new byte[bufferSize];
            ReadProcessMemory(this.Handle, (IntPtr)address, array, bufferSize, 0);
            string text = enc.GetString(array);
            bool flag = text.Contains('\0');
            if (flag)
            {
                text = text.Substring(0, text.IndexOf('\0'));
            }
            return text;
        }
        /// <summary>
        /// Writes values to memory
        /// </summary>
        /// <typeparam name="T">The type of data to write to memory</typeparam>
        public void Write<T>(IntPtr address, T value)
        {
            if (Permissions != MemoryPermissions.All || Permissions != MemoryPermissions.Writeonly) throw new Exception("Memory access denied: permission does not match the method being called");
            int num = Marshal.SizeOf<T>();
            byte[] array = new byte[num];
            IntPtr intPtr = Marshal.AllocHGlobal(num);
            Marshal.StructureToPtr<T>(value, intPtr, true);
            Marshal.Copy(intPtr, array, 0, num);
            Marshal.FreeHGlobal(intPtr);
            WriteProcessMemory(this.Handle, address, array, num, 0);
        }
        /// <summary>
        /// Writes values to memory
        /// </summary>
        /// <typeparam name="T">The type of data to write to memory</typeparam>
        public void Write<T>(int address, T value)
        {
            if (Permissions != MemoryPermissions.All || Permissions != MemoryPermissions.Writeonly) throw new Exception("Memory access denied: permission does not match the method being called");
            int num = Marshal.SizeOf<T>();
            byte[] array = new byte[num];
            IntPtr intPtr = Marshal.AllocHGlobal(num);
            Marshal.StructureToPtr<T>(value, intPtr, true);
            Marshal.Copy(intPtr, array, 0, num);
            Marshal.FreeHGlobal(intPtr);
            WriteProcessMemory(this.Handle, (IntPtr)address, array, num, 0);
        }
        /// <summary>
        /// Reads the bytes array from memory
        /// </summary>
        public byte[] ReadBytes(int address, int length)
        {
            if (Permissions != MemoryPermissions.All || Permissions != MemoryPermissions.Readonly) throw new Exception("Memory access denied: permission does not match the method being called");
            byte[] array = new byte[length];
            ReadProcessMemory(this.Handle, (IntPtr)address, array, length, 0);
            return array;
        }
        /// <summary>
        /// Writes an array of bytes to memory
        /// </summary>
        public void WriteBytes(int address, byte[] value)
        {
            if (Permissions != MemoryPermissions.All || Permissions != MemoryPermissions.Writeonly) throw new Exception("Memory access denied: permission does not match the method being called");
            WriteProcessMemory(this.Handle, (IntPtr)address, value, value.Length, 0);
        }




        private byte[] GetPattern(string pattern)
        {
            bool flag = string.IsNullOrWhiteSpace(pattern);
            if (flag)
            {
                throw new ArgumentNullException("pattern");
            }
            List<byte> list = new List<byte>(pattern.Length);
            List<string> list2 = pattern.Split(new char[]
            {
        ' '
            }).ToList<string>();
            list2.RemoveAll((string str) => str == "");
            foreach (string text in list2)
            {
                bool flag2 = text == "?";
                if (flag2)
                {
                    list.Add(0);
                }
                else
                {
                    list.Add(Convert.ToByte(text, 16));
                }
            }
            return list.ToArray();
        }
        private T GetStruct<T>(byte[] bytes)
        {
            GCHandle gchandle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            T result = (T)((object)Marshal.PtrToStructure(gchandle.AddrOfPinnedObject(), typeof(T)));
            gchandle.Free();
            return result;
        }
        [SuppressUnmanagedCodeSecurity]
        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);
        [SuppressUnmanagedCodeSecurity]
        [DllImport("kernel32.dll")]
        private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] buffer, int size, int lpNumberOfBytesWritten);
        [SuppressUnmanagedCodeSecurity]
        [DllImport("kernel32.dll")]
        private static extern bool ReadProcessMemory(IntPtr Handle, IntPtr Address, byte[] buffer, int Size, int BytesRead = 0);
    }
}
