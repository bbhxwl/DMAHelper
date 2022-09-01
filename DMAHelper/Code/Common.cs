﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using vmmsharp;

namespace DMAHelper
{
    public  class Common
    {

       public delegate ulong decryptData(ulong c);
 
        
        
        public static  uint dec_objid(int value)
        {
            uint v18 = _ROR4_oL_((uint)(value ^ pubg.Offset_XorKey1), pubg.Offset_RorValue, pubg.Offset_IsingRor);
            return v18 ^ (v18 << 16) ^ (uint)pubg.Offset_XorKey2;
        }

        static uint _ROR4_oL_(uint x, int count, bool IsRor)
        {
            count %= 32;
            if (IsRor)
                return (x << (32 - count)) | (x >> count);
            else
                return (x << count) | (x >> (32 - count));
        }

        [DllImport("kernel32.dll", SetLastError = true)]
      public   static extern IntPtr VirtualAlloc(IntPtr lpAddress,int dwSize, AllocationType lAllocationType, MemoryProtection flProtect);

        
    }
    [Flags]
    public enum AllocationType
    {
        Commit = 0x1000,
        Reserve = 0x2000,
        Decommit = 0x4000,
        Release = 0x8000,
        Reset = 0x80000,
        Physical = 0x400000,
        TopDown = 0x100000,
        WriteWatch = 0x200000,
        LargePages = 0x20000000
    }

    [Flags]
    public enum MemoryProtection
    {
        Execute = 0x10,
        ExecuteRead = 0x20,
        ExecuteReadWrite = 0x40,
        ExecuteWriteCopy = 0x80,
        NoAccess = 0x01,
        ReadOnly = 0x02,
        ReadWrite = 0x04,
        WriteCopy = 0x08,
        GuardModifierflag = 0x100,
        NoCacheModifierflag = 0x200,
        WriteCombineModifierflag = 0x400
    }
}
