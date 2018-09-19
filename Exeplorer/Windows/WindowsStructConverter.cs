using System;
using System.Text;

namespace Exeplorer.Windows {
    public static class WindowsStructConverter {
        public static NtHeader ToNtHeader(byte[] data, int offset) {
            return new NtHeader() {
                Signature = BitConverter.ToUInt32(data, offset),
                FileHeader = ToFileHeader(data, offset + sizeof(uint)),
                OptionalHeader = ToOptionalHeader(data, offset + sizeof(uint) + H.IMAGE_SIZEOF_FILE_HEADER)
            };
        }

        public static OptionalHeader ToOptionalHeader(byte[] data, int offset) {
            var magic = BitConverter.ToUInt16(data, offset);

            var optHeader = new OptionalHeader() {
                Magic = magic,
                MajorLinkerVersion = data[offset + 2],
                MinorLinkerVersion = data[offset + 3],
                SizeOfCode = BitConverter.ToUInt32(data, offset + 4),
                SizeOfInitializedData = BitConverter.ToUInt32(data, offset + 8),
                SizeOfUninitializedData = BitConverter.ToUInt32(data, offset + 12),
                AddressOfEntryPoint = BitConverter.ToUInt32(data, offset + 16),
                BaseOfCode = BitConverter.ToUInt32(data, offset + 20),
                SectionAlignment = BitConverter.ToUInt32(data, offset + 32),
                FileAlignment = BitConverter.ToUInt32(data, offset + 36),
                MajorOperatingSystemVersion = BitConverter.ToUInt16(data, offset + 40),
                MinorOperatingSystemVersion = BitConverter.ToUInt16(data, offset + 42),
                MajorImageVersion = BitConverter.ToUInt16(data, offset + 44),
                MinorImageVersion = BitConverter.ToUInt16(data, offset + 46),
                MajorSubsystemVersion = BitConverter.ToUInt16(data, offset + 48),
                MinorSubsystemVersion = BitConverter.ToUInt16(data, offset + 50),
                Win32VersionValue = BitConverter.ToUInt32(data, offset + 52),
                SizeOfImage = BitConverter.ToUInt32(data, offset + 56),
                SizeOfHeaders = BitConverter.ToUInt32(data, offset + 60),
                CheckSum = BitConverter.ToUInt32(data, offset + 64),
                Subsystem = BitConverter.ToUInt16(data, offset + 68),
                DllCharacteristics = BitConverter.ToUInt16(data, offset + 70)
            };

            if (magic == H.IMAGE_NT_OPTIONAL_HDR32_MAGIC) {
                optHeader.ImageBase = BitConverter.ToUInt32(data, offset + 28);
                optHeader.SizeOfStackReserve = BitConverter.ToUInt32(data, offset + 72);
                optHeader.SizeOfStackCommit = BitConverter.ToUInt32(data, offset + 76);
                optHeader.SizeOfHeapReserve = BitConverter.ToUInt32(data, offset + 80);
                optHeader.SizeOfHeapCommit = BitConverter.ToUInt32(data, offset + 84);
                optHeader.LoaderFlags = BitConverter.ToUInt32(data, offset + 88);
                optHeader.NumberOfRvaAndSizes = BitConverter.ToUInt32(data, offset + 92);
            }
            else {
                optHeader.ImageBase = BitConverter.ToUInt64(data, offset + 24);
                optHeader.SizeOfStackReserve = BitConverter.ToUInt64(data, offset + 72);
                optHeader.SizeOfStackCommit = BitConverter.ToUInt64(data, offset + 80);
                optHeader.SizeOfHeapReserve = BitConverter.ToUInt64(data, offset + 88);
                optHeader.SizeOfHeapCommit = BitConverter.ToUInt64(data, offset + 96);
                optHeader.LoaderFlags = BitConverter.ToUInt32(data, offset + 104);
                optHeader.NumberOfRvaAndSizes = BitConverter.ToUInt32(data, offset + 108);
            }

            var directoriesOffset = magic == H.IMAGE_NT_OPTIONAL_HDR32_MAGIC ? 96 : 112;
            optHeader.DataDirectories = new DataDirectory[H.IMAGE_NUMBEROF_DIRECTORY_ENTRIES];

            for(var i = 0; i < H.IMAGE_NUMBEROF_DIRECTORY_ENTRIES; ++i) {
                optHeader.DataDirectories[i] = ToDataDirectory(data, offset + directoriesOffset);
                directoriesOffset += H.IMAGE_SIZEOF_DIRECTORY_ENTRY;
            }

            return optHeader;
        }

        public static OptionalHeader32 ToOptionalHeader32(byte[] data, int offset) {
            return new OptionalHeader32() {
                Magic = BitConverter.ToUInt16(data, offset),
                MajorLinkerVersion = data[offset + 2],
                MinorLinkerVersion = data[offset + 3],
                SizeOfCode = BitConverter.ToUInt32(data, offset + 4),
                SizeOfInitializedData = BitConverter.ToUInt32(data, offset + 8),
                SizeOfUninitializedData = BitConverter.ToUInt32(data, offset + 12),
                AddressOfEntryPoint = BitConverter.ToUInt32(data, offset + 16),
                BaseOfCode = BitConverter.ToUInt32(data, offset + 20),
                BaseOfData = BitConverter.ToUInt32(data, offset + 24),
                ImageBase = BitConverter.ToUInt32(data, offset + 28),
                SectionAlignment = BitConverter.ToUInt32(data, offset + 32),
                FileAlignment = BitConverter.ToUInt32(data, offset + 36),
                MajorOperatingSystemVersion = BitConverter.ToUInt16(data, offset + 40),
                MinorOperatingSystemVersion = BitConverter.ToUInt16(data, offset + 42),
                MajorImageVersion = BitConverter.ToUInt16(data, offset + 44),
                MinorImageVersion = BitConverter.ToUInt16(data, offset + 46),
                MajorSubsystemVersion = BitConverter.ToUInt16(data, offset + 48),
                MinorSubsystemVersion = BitConverter.ToUInt16(data, offset + 50),
                Win32VersionValue = BitConverter.ToUInt32(data, offset + 52),
                SizeOfImage = BitConverter.ToUInt32(data, offset + 56),
                SizeOfHeaders = BitConverter.ToUInt32(data, offset + 60),
                CheckSum = BitConverter.ToUInt32(data, offset + 64),
                Subsystem = BitConverter.ToUInt16(data, offset + 68),
                DllCharacteristics = BitConverter.ToUInt16(data, offset + 70),
                SizeOfStackReserve = BitConverter.ToUInt32(data, offset + 72),
                SizeOfStackCommit = BitConverter.ToUInt32(data, offset + 76),
                SizeOfHeapReserve = BitConverter.ToUInt32(data, offset + 80),
                SizeOfHeapCommit = BitConverter.ToUInt32(data, offset + 84),
                LoaderFlags = BitConverter.ToUInt32(data, offset + 88),
                NumberOfRvaAndSizes = BitConverter.ToUInt32(data, offset + 92),
            };
        }

        public static OptionalHeader64 ToOptionalHeader64(byte[] data, int offset) {
            return new OptionalHeader64() {
                Magic = BitConverter.ToUInt16(data, offset),
                MajorLinkerVersion = data[offset + 2],
                MinorLinkerVersion = data[offset + 3],
                SizeOfCode = BitConverter.ToUInt32(data, offset + 4),
                SizeOfInitializedData = BitConverter.ToUInt32(data, offset + 8),
                SizeOfUninitializedData = BitConverter.ToUInt32(data, offset + 12),
                AddressOfEntryPoint = BitConverter.ToUInt32(data, offset + 16),
                BaseOfCode = BitConverter.ToUInt32(data, offset + 20),
                ImageBase = BitConverter.ToUInt64(data, offset + 24),
                SectionAlignment = BitConverter.ToUInt32(data, offset + 32),
                FileAlignment = BitConverter.ToUInt32(data, offset + 36),
                MajorOperatingSystemVersion = BitConverter.ToUInt16(data, offset + 40),
                MinorOperatingSystemVersion = BitConverter.ToUInt16(data, offset + 42),
                MajorImageVersion = BitConverter.ToUInt16(data, offset + 44),
                MinorImageVersion = BitConverter.ToUInt16(data, offset + 46),
                MajorSubsystemVersion = BitConverter.ToUInt16(data, offset + 48),
                MinorSubsystemVersion = BitConverter.ToUInt16(data, offset + 50),
                Win32VersionValue = BitConverter.ToUInt32(data, offset + 52),
                SizeOfImage = BitConverter.ToUInt32(data, offset + 56),
                SizeOfHeaders = BitConverter.ToUInt32(data, offset + 60),
                CheckSum = BitConverter.ToUInt32(data, offset + 64),
                Subsystem = BitConverter.ToUInt16(data, offset + 68),
                DllCharacteristics = BitConverter.ToUInt16(data, offset + 70),
                SizeOfStackReserve = BitConverter.ToUInt64(data, offset + 72),
                SizeOfStackCommit = BitConverter.ToUInt64(data, offset + 80),
                SizeOfHeapReserve = BitConverter.ToUInt64(data, offset + 88),
                SizeOfHeapCommit = BitConverter.ToUInt64(data, offset + 96),
                LoaderFlags = BitConverter.ToUInt32(data, offset + 104),
                NumberOfRvaAndSizes = BitConverter.ToUInt32(data, offset + 108)
            };
        }

        public static SectionHeader ToSectionHeader(byte[] data, int offset) {
            var misc = BitConverter.ToUInt32(data, offset + H.IMAGE_SIZEOF_SECTION_NAME);

            return new SectionHeader() {
                Name = Encoding.UTF8.GetString(data, offset, H.IMAGE_SIZEOF_SECTION_NAME).TrimEnd('\0'),
                Misc = new SectionHeaderMiscellaneous(BitConverter.ToUInt32(data, offset + 8)),
                VirtualAddress = BitConverter.ToUInt32(data, offset + 12),
                SizeOfRawData = BitConverter.ToUInt32(data, offset + 16),
                PointerToRawData = BitConverter.ToUInt32(data, offset + 20),
                PointerToRelocations = BitConverter.ToUInt32(data, offset + 24),
                PointerToLinenumbers = BitConverter.ToUInt32(data, offset + 28),
                NumberOfRelocations = BitConverter.ToUInt16(data, offset + 32),
                NumberOfLinenumbers = BitConverter.ToUInt16(data, offset + 34),
                Characteristics = BitConverter.ToUInt32(data, offset + 36)
            };
        }

        public static FileHeader ToFileHeader(byte[] data, int offset) {
            return new FileHeader() {
                Machine = BitConverter.ToUInt16(data, offset),
                NumberOfSections = BitConverter.ToUInt16(data, offset + 2),
                TimeDateStamp = BitConverter.ToUInt32(data, offset + 4),
                PointerToSymbolTable = BitConverter.ToUInt32(data, offset + 8),
                NumberOfSymbols = BitConverter.ToUInt32(data, offset + 12),
                SizeOfOptionalHeader = BitConverter.ToUInt16(data, offset + 16),
                Characteristics = BitConverter.ToUInt16(data, offset + 18)
            };
        }

        public static DataDirectory ToDataDirectory(byte[] data, int offset) {
            return new DataDirectory() {
                VirtualAddress = BitConverter.ToUInt32(data, offset),
                Size = BitConverter.ToUInt32(data, offset + sizeof(uint))
            };
        }

        public static ImportDescriptor ToImportDescriptor(byte[] data, int offset) {
            return new ImportDescriptor() {
                OriginalFirstThunkRva = BitConverter.ToUInt32(data, offset),
                TimeDateStamp = BitConverter.ToUInt32(data, offset + 4),
                ForwarderChain = BitConverter.ToUInt32(data, offset + 8),
                Name = BitConverter.ToUInt32(data, offset + 12),
                FirstThunkRva = BitConverter.ToUInt32(data, offset + 16)
            };
        }

        public static ExportDirectory ToExportDirectory(byte[] data, int offset) {
            return new ExportDirectory() {
                Characteristics = BitConverter.ToUInt32(data, offset),
                TimeDateStamp = BitConverter.ToUInt32(data, offset + 4),
                MajorVersion = BitConverter.ToUInt16(data, offset + 8),
                MinorVersion = BitConverter.ToUInt16(data, offset + 10),
                Name = BitConverter.ToUInt32(data, offset + 12),
                Base = BitConverter.ToUInt32(data, offset + 16),
                NumberOfFunctions = BitConverter.ToUInt32(data, offset + 20),
                NumberOfNames = BitConverter.ToUInt32(data, offset + 24),
                AddressOfFunctions = BitConverter.ToUInt32(data, offset + 28),
                AddressOfNames = BitConverter.ToUInt32(data, offset + 32),
                AddressOfNameOrdinals = BitConverter.ToUInt32(data, offset + 36)
            };
        }
    }
}
