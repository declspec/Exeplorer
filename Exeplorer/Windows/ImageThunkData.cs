using System.Runtime.InteropServices;

namespace Exeplorer.Windows {
    public struct ImageThunkData64 {
        public ImageThunkData64U1 U1;
    }
    
    [StructLayout(LayoutKind.Explicit)]
    public struct ImageThunkData64U1 {
        [FieldOffset(0)]
        public ulong ForwarderString;
        [FieldOffset(0)]
        public ulong Function;
        [FieldOffset(0)]
        public ulong Ordinal;
        [FieldOffset(0)]
        public ulong AddressOfData;
    }

    public struct ImageThunkData32 {
        public ImageThunkData32U1 U1;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct ImageThunkData32U1 {
        [FieldOffset(0)]
        public uint ForwarderString;
        [FieldOffset(0)]
        public uint Function;
        [FieldOffset(0)]
        public uint Ordinal;
        [FieldOffset(0)]
        public uint AddressOfData;
    }
}
