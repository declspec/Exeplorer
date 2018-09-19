using System.Runtime.InteropServices;

namespace Exeplorer.Windows {
    public class SectionHeader {
        public string Name;
        public SectionHeaderMiscellaneous Misc;
        public uint VirtualAddress;
        public uint SizeOfRawData;
        public uint PointerToRawData;
        public uint PointerToRelocations;
        public uint PointerToLinenumbers;
        public ushort NumberOfRelocations;
        public ushort NumberOfLinenumbers;
        public uint Characteristics;

        public override string ToString() {
            return Name;
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct SectionHeaderMiscellaneous {
        [FieldOffset(0)]
        public uint PhysicalAddress;
        [FieldOffset(0)]
        public uint VirtualSize;

        public SectionHeaderMiscellaneous(uint misc) {
            PhysicalAddress = VirtualSize = misc;
        }
    }
}
