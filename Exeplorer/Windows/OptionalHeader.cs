namespace Exeplorer.Windows {
    public class OptionalHeader {
        public ushort Magic;                        //  0
        public byte MajorLinkerVersion;             //  2
        public byte MinorLinkerVersion;             //  3
        public uint SizeOfCode;                     //  4
        public uint SizeOfInitializedData;          //  8
        public uint SizeOfUninitializedData;        // 12
        public uint AddressOfEntryPoint;            // 16
        public uint BaseOfCode;                     // 20
        public ulong ImageBase;                     // 24
        public uint SectionAlignment;               // 32
        public uint FileAlignment;                  // 36
        public ushort MajorOperatingSystemVersion;  // 40
        public ushort MinorOperatingSystemVersion;  // 42
        public ushort MajorImageVersion;            // 44
        public ushort MinorImageVersion;            // 46
        public ushort MajorSubsystemVersion;        // 48
        public ushort MinorSubsystemVersion;        // 50
        public uint Win32VersionValue;              // 52
        public uint SizeOfImage;                    // 56
        public uint SizeOfHeaders;                  // 60
        public uint CheckSum;                       // 64
        public ushort Subsystem;                    // 68
        public ushort DllCharacteristics;           // 70
        public ulong SizeOfStackReserve;            // 72
        public ulong SizeOfStackCommit;             // 76
        public ulong SizeOfHeapReserve;             // 
        public ulong SizeOfHeapCommit;              // 
        public uint LoaderFlags;                    // 
        public uint NumberOfRvaAndSizes;            // 
        public DataDirectory[] DataDirectories;
    };

    public class OptionalHeader64 {
        public const int Size = 240;

        public ushort Magic;                        //  0
        public byte MajorLinkerVersion;             //  2
        public byte MinorLinkerVersion;             //  3
        public uint SizeOfCode;                     //  4
        public uint SizeOfInitializedData;          //  8
        public uint SizeOfUninitializedData;        // 12
        public uint AddressOfEntryPoint;            // 16
        public uint BaseOfCode;                     // 20
        public ulong ImageBase;                     // 24
        public uint SectionAlignment;               // 32
        public uint FileAlignment;                  // 36
        public ushort MajorOperatingSystemVersion;  // 40
        public ushort MinorOperatingSystemVersion;  // 42
        public ushort MajorImageVersion;            // 44
        public ushort MinorImageVersion;            // 46
        public ushort MajorSubsystemVersion;        // 48
        public ushort MinorSubsystemVersion;        // 50
        public uint Win32VersionValue;              // 52
        public uint SizeOfImage;                    // 56
        public uint SizeOfHeaders;                  // 60
        public uint CheckSum;                       // 64
        public ushort Subsystem;                    // 68
        public ushort DllCharacteristics;           // 70
        public ulong SizeOfStackReserve;            // 72
        public ulong SizeOfStackCommit;             // 80
        public ulong SizeOfHeapReserve;             // 88
        public ulong SizeOfHeapCommit;              // 96
        public uint LoaderFlags;                    // 104
        public uint NumberOfRvaAndSizes;            // 108
        public DataDirectory[] DataDirectories; // 112
    };

    public class OptionalHeader32 {
        public const int Size = 224;

        public ushort Magic;                        //  0
        public byte MajorLinkerVersion;             //  2
        public byte MinorLinkerVersion;             //  3
        public uint SizeOfCode;                     //  4
        public uint SizeOfInitializedData;          //  8
        public uint SizeOfUninitializedData;        // 12
        public uint AddressOfEntryPoint;            // 16
        public uint BaseOfCode;                     // 20
        public uint BaseOfData;                     // 24
        public uint ImageBase;                      // 28
        public uint SectionAlignment;               // 32
        public uint FileAlignment;                  // 36
        public ushort MajorOperatingSystemVersion;  // 40
        public ushort MinorOperatingSystemVersion;  // 42
        public ushort MajorImageVersion;            // 44
        public ushort MinorImageVersion;            // 46
        public ushort MajorSubsystemVersion;        // 48
        public ushort MinorSubsystemVersion;        // 50
        public uint Win32VersionValue;              // 52
        public uint SizeOfImage;                    // 56
        public uint SizeOfHeaders;                  // 60
        public uint CheckSum;                       // 64
        public ushort Subsystem;                    // 68
        public ushort DllCharacteristics;           // 70
        public uint SizeOfStackReserve;             // 72
        public uint SizeOfStackCommit;              // 76
        public uint SizeOfHeapReserve;              // 80
        public uint SizeOfHeapCommit;               // 84
        public uint LoaderFlags;                    // 88
        public uint NumberOfRvaAndSizes;            // 92
        public DataDirectory[] DataDirectories; // 96
    };
}
