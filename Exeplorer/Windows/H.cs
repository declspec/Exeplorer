namespace Exeplorer.Windows {
    public static class H {
        public const int IMAGE_SIZEOF_FILE_HEADER = 0x14;
        public const int IMAGE_SIZEOF_SECTION_HEADER = 0x28;
        public const int IMAGE_SIZEOF_DIRECTORY_ENTRY = 0x08;
        public const int IMAGE_SIZEOF_DOS_HEADER = 0x40;
        public const int IMAGE_SIZEOF_SECTION_NAME = 0x08;
        public const int IMAGE_SIZEOF_IMPORT_DESCRIPTOR = 0x14;

        /// <summary>Size of a PE32 Optional Header, minus the size of the data directories.</summary>
        public const int IMAGE_SIZEOF_OPTIONAL_HEADER32 = 0x60;
        /// <summary>Size of a PE32+ Optional Header, minus the size of the data directories.</summary>
        public const int IMAGE_SIZEOF_OPTIONAL_HEADER64 = 0x70;
        public const int IMAGE_NUMBEROF_DIRECTORY_ENTRIES = 0x10;

        public const ushort IMAGE_NT_OPTIONAL_HDR32_MAGIC = 0x10B;
        public const ushort IMAGE_NT_OPTIONAL_HDR64_MAGIC = 0x20B;

        public const ushort IMAGE_DOS_SIGNATURE = 0x5A4D;
        public const uint IMAGE_NT_SIGNATURE = 0x4550;

        public const ulong IMAGE_ORDINAL_FLAG64 = 0x8000000000000000;
        public const uint IMAGE_ORDINAL_FLAG32 = 0x80000000;

        public const int IMAGE_DIRECTORY_ENTRY_EXPORT = 0; // Export directory
        public const int IMAGE_DIRECTORY_ENTRY_IMPORT = 1; // Import directory
        public const int IMAGE_DIRECTORY_ENTRY_RESOURCE = 2; // Resource directory
        public const int IMAGE_DIRECTORY_ENTRY_EXCEPTION = 3; // Exception directory
        public const int IMAGE_DIRECTORY_ENTRY_SECURITY = 4; // Security directory
        public const int IMAGE_DIRECTORY_ENTRY_BASERELOC = 5; // Base relocation table
        public const int IMAGE_DIRECTORY_ENTRY_DEBUG = 6; // Debug directory
        public const int IMAGE_DIRECTORY_ENTRY_ARCHITECTURE = 7; // Architecture-specific data
        public const int IMAGE_DIRECTORY_ENTRY_GLOBALPTR = 8; // The relative virtual address of global pointer
        public const int IMAGE_DIRECTORY_ENTRY_TLS = 9; // Thread local storage directory
        public const int IMAGE_DIRECTORY_ENTRY_LOAD_CONFIG = 10; // Load configuration directory
        public const int IMAGE_DIRECTORY_ENTRY_BOUND_IMPORT = 11; // Bound import directory
        public const int IMAGE_DIRECTORY_ENTRY_IAT = 12; // Import address table
        public const int IMAGE_DIRECTORY_ENTRY_DELAY_IMPORT = 13; // Delay import table
        public const int IMAGE_DIRECTORY_ENTRY_COM_DESCRIPTOR = 14; // COM descriptor table
    }
}
