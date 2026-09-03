using Microsoft.VisualBasic.ApplicationServices;
using System;
using System.Diagnostics;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace repositorioo
{
    public class MemoryEngine
    {
        [DllImport("user32.dll")] public static extern short GetAsyncKeyState(int vKey);
        [DllImport("kernel32.dll")] public static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);
        [DllImport("kernel32.dll")] public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, out IntPtr lpNumberOfBytesWritten);
        [DllImport("kernel32.dll")] public static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);
        [DllImport("kernel32.dll")] public static extern uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);
        [DllImport("kernel32.dll")] public static extern bool CloseHandle(IntPtr hObject);
        [DllImport("kernel32.dll", SetLastError = true)] public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, out int lpNumberOfBytesRead);
        [DllImport("kernel32.dll")] public static extern bool VirtualFreeEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint dwFreeType);
        [DllImport("kernel32.dll", SetLastError = true)] public static extern IntPtr OpenProcess(uint processAccess, bool bInheritHandle, int processId);

        public IntPtr HProcess { get; private set; } = IntPtr.Zero;
        public IntPtr BaseAddress { get; private set; } = IntPtr.Zero;
        public IntPtr FunctionSwap { get; private set; } = IntPtr.Zero;
        public IntPtr FunctionSwap2 { get; private set; } = IntPtr.Zero;
        public IntPtr SkillCall { get; private set; } = IntPtr.Zero;
        public int elmoAtual { get; private set; } = 0;
        public int capaAtual { get; private set; } = 0;
        public int ornAtual { get; private set; } = 0;

        private CancellationTokenSource? _cts;
        private bool _verificacaoAtiva = false;

        public bool Conectar() {
            Process[] processes = Process.GetProcessesByName("elementclient_64");
            if (processes.Length == 0) return false;

            Process gameProcess = processes[0];
            HProcess = OpenProcess(0x001F0FFF, false, gameProcess.Id);

            if (HProcess == IntPtr.Zero) return false;


            BaseAddress = gameProcess.MainModule.BaseAddress;
            FunctionSwap = IntPtr.Add(BaseAddress, 0xA7F8D0);
            FunctionSwap2 = IntPtr.Add(BaseAddress, 0xA6FF10);
            SkillCall = IntPtr.Add(BaseAddress, 0x8E6FB0);

            try {
                IntPtr ptr1 = (IntPtr)ReadInt64(BaseAddress + 0x01733D70);
                if (ptr1 == IntPtr.Zero) return;

                IntPtr ptr2 = (IntPtr)ReadInt64(ptr1 + 0x10);
                if (ptr2 == IntPtr.Zero) return;

                IntPtr BaseBolsa = (IntPtr)ReadInt64(BaseAddress + 0x01734248);
                if (BaseBolsa == IntPtr.Zero) return;

                IntPtr ptrBolsa = (IntPtr)ReadInt64(BaseBolsa + 0x60);
                if (ptrBolsa == IntPtr.Zero) return;

            }
            catch {
                elmoAtual = 0;
                capaAtual = 0;
                ornAtual = 0;

            }

            return true;
            }
    }

        public void RelerItensAtuais()
        {
            if (HProcess == IntPtr.Zero) return;

            try {
                elmoAtual = ReadInt32(ptr2 + 0x24);
                capaAtual = ReadInt32(ptr2 + 0x2C);
                ornAtual = ReadInt32(ptr2 + 0x34);

            }
            catch {
                elmoAtual = 0;
                capaAtual = 0;
                ornAtual = 0;

            }
        }

        public void DesconectarEParar()
        {
            _cts?.Cancel();
            _verificacaoAtiva = false;
        }

        public void ProcessarTrocaDeSet(ulong defElmo, ulong defCapa, ulong defOrn, ulong atkElmo, ulong atkCapa, ulong atkOrn)
        {
            if (HProcess == IntPtr.Zero) return;

            elmoAtual = ReadInt32(ptr2 + 0x24);
            capaAtual = ReadInt32(ptr2 + 0x2C);
            ornAtual = ReadInt32(ptr2 + 0x34);

            if ((ulong)elmoAtual == defElmo && (ulong)capaAtual == defCapa && (ulong)ornAtual == defOrn)
            {
                TrocarSetCompleto(atkOrn, atkElmo, atkCapa);
            }
            else if ((ulong)elmoAtual == atkElmo && (ulong)capaAtual == atkCapa && (ulong)ornAtual == atkOrn)
            {
                TrocarSetCompleto(defOrn, defElmo, defCapa);
            }
            else
            {
                TrocarSetCompleto(defOrn, defElmo, defCapa);
            }
        }

        public void TrocarSetCompleto(ulong idOrn, ulong idElmo, ulong idCapa)
        {
            if (HProcess == IntPtr.Zero || FunctionSwap == IntPtr.Zero) return;

            byte[] shellcode = new byte[]
            {
                0x48, 0x83, 0xEC, 0x28,                                     
                
                // Ornamento 0x05
                0x48, 0xB9, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x48, 0xBA, 0x05, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x48, 0xB8, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0xFF, 0xD0,                                                 
                
                // Elmo 0x01
                0x48, 0xB9, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x48, 0xBA, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x48, 0xB8, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0xFF, 0xD0,                                                 
                
                // Capa 0x03
                0x48, 0xB9, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x48, 0xBA, 0x03, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x48, 0xB8, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0xFF, 0xD0,

                0x48, 0x83, 0xC4, 0x28,
                0xC3
            };

            Buffer.BlockCopy(BitConverter.GetBytes(idOrn), 0, shellcode, 6, 8);
            Buffer.BlockCopy(BitConverter.GetBytes((ulong)FunctionSwap), 0, shellcode, 26, 8);

            Buffer.BlockCopy(BitConverter.GetBytes(idElmo), 0, shellcode, 38, 8);
            Buffer.BlockCopy(BitConverter.GetBytes((ulong)FunctionSwap), 0, shellcode, 58, 8);

            Buffer.BlockCopy(BitConverter.GetBytes(idCapa), 0, shellcode, 70, 8);
            Buffer.BlockCopy(BitConverter.GetBytes((ulong)FunctionSwap), 0, shellcode, 90, 8);

            IntPtr allocMem = VirtualAllocEx(HProcess, IntPtr.Zero, (uint)shellcode.Length, 0x1000 | 0x2000, 0x40);
            if (allocMem != IntPtr.Zero)
            {
                WriteProcessMemory(HProcess, allocMem, shellcode, (uint)shellcode.Length, out _);
                IntPtr hThread = CreateRemoteThread(HProcess, IntPtr.Zero, 0, allocMem, IntPtr.Zero, 0, IntPtr.Zero);

                if (hThread != IntPtr.Zero)
                {
                    WaitForSingleObject(hThread, 1000);
                    CloseHandle(hThread);
                }
                VirtualFreeEx(HProcess, allocMem, 0, 0x8000);
            }
        }

        // public void teste2(ulong rcx, ulong rdx, ulong r8, ulong r9)
        // {
        //     byte[] shellcode = new byte[]
        //     {
        //         0x48, 0x83, 0xEC, 0x20, //[0-3] SUB RSP, 20h
        //         0x48, 0xB9, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, //[4-13] mov RCX
        //         0x48, 0xBA, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, //[14-23] mov RDX
        //         0x49, 0xB8, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, //[24-33] mov R8
        //         0x49, 0xB9, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, //[34-43] mov R9
        //         0x48, 0xB8, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, //[44-53] mov rax
        //         0xFF, 0xD0,
        //         0x48, 0x83, 0xC4, 0x20,
        //         0xC3
        //     };

        //     Buffer.BlockCopy(BitConverter.GetBytes((ulong)rcx), 0, shellcode, 6, 8);
        //     Buffer.BlockCopy(BitConverter.GetBytes((ulong)rdx), 0, shellcode, 16, 8);
        //     Buffer.BlockCopy(BitConverter.GetBytes((ulong)r8), 0, shellcode, 26, 8);
        //     Buffer.BlockCopy(BitConverter.GetBytes((ulong)r9), 0, shellcode, 36, 8);
        //     Buffer.BlockCopy(BitConverter.GetBytes((ulong)SkillCall), 0, shellcode, 46, 8);


        //     IntPtr allocMem = VirtualAllocEx(HProcess, IntPtr.Zero, (uint)shellcode.Length, 0x1000 | 0x2000, 0x40);
        //     WriteProcessMemory(HProcess, allocMem, shellcode, (uint)shellcode.Length, out _);

        //     IntPtr hThread = CreateRemoteThread(HProcess, IntPtr.Zero, 0, allocMem, IntPtr.Zero, 0, IntPtr.Zero);
        //     WaitForSingleObject(hThread, 0xFFFFFFFF);
        //     CloseHandle(hThread);
        //     VirtualFreeEx(HProcess, allocMem, 0, 0x8000);
        // }

        public int ReadInt32(IntPtr address)
        {
            byte[] buffer = new byte[4];
            ReadProcessMemory(HProcess, address, buffer, 4, out _);
            return BitConverter.ToInt32(buffer, 0);
        }

        public long ReadInt64(IntPtr address)
        {
            byte[] buffer = new byte[8];
            ReadProcessMemory(HProcess, address, buffer, 8, out _);
            return BitConverter.ToInt64(buffer, 0);
        }
    }
}