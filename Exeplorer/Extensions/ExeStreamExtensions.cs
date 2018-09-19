using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Exeplorer.IO;
using Exeplorer.Windows;

namespace Exeplorer.Extensions {
    public static class ExeStreamExtensions {
        private static readonly Func<byte[], int, ulong> ToThunk32 = (buffer, offset) => BitConverter.ToUInt32(buffer, offset);
        private static readonly Func<byte[], int, ulong> ToThunk64 = BitConverter.ToUInt64;

        public struct ImportedFunction {
            public ushort OrdinalOrHint;
            public string Name;
        }

        public static ExportDirectory ReadExportDirectory(this ExeStream stream) {
            var entry = stream.OptionalHeader.DataDirectories[H.IMAGE_DIRECTORY_ENTRY_EXPORT];
            if (entry.Size == 0)
                return null;

            var buffer = new byte[ExportDirectory.Size];
            stream.SeekRva(entry.VirtualAddress);
            stream.FullRead(buffer, 0, buffer.Length);

            return WindowsStructConverter.ToExportDirectory(buffer, 0);
        }

        public static IList<ExportedFunction> ReadExportAddressTable(this ExeStream stream, ExportDirectory directory) {
            var size = (directory.NumberOfFunctions * sizeof(uint)) + (directory.NumberOfNames * (sizeof(uint) + sizeof(ushort)));
            var buffer = new byte[size];

            // Precompute offsets within the buffer.
            var addressOffset = 0;
            var namesOffset = (int)(directory.NumberOfFunctions * sizeof(uint));
            var ordinalsOffset = (int)(namesOffset + (directory.NumberOfNames * sizeof(uint)));

            // Read the data in (will need 3 reads)
            stream.SeekRva(directory.AddressOfFunctions);
            stream.FullRead(buffer, 0, namesOffset);
            stream.SeekRva(directory.AddressOfNames);
            stream.FullRead(buffer, namesOffset, ordinalsOffset - namesOffset);
            stream.SeekRva(directory.AddressOfNameOrdinals);
            stream.FullRead(buffer, ordinalsOffset, buffer.Length - ordinalsOffset);

            // Bit janky, but we guarantee the ordering to match the lexicographical ordering from the names table.
            var functions = new ExportedFunction[directory.NumberOfFunctions];

            for(var i = 0; i < directory.NumberOfNames; ++i) {
                var index = BitConverter.ToUInt16(buffer, ordinalsOffset + (i * sizeof(ushort)));

                functions[i] = new ExportedFunction() {
                    Name = BitConverter.ToUInt32(buffer, namesOffset + (i * sizeof(uint))),
                    Ordinal = (ushort)(index + directory.Base),
                    VirtualAddressOrForwarder = BitConverter.ToUInt32(buffer, addressOffset + (index * sizeof(uint)))
                };
            }

            // Fill in the gaps
            for(var i = 0; i < directory.NumberOfFunctions; ++i) {
                if (functions[i] != null)
                    continue;

                functions[i] = new ExportedFunction() {
                    Ordinal = (ushort)(i + directory.Base),
                    VirtualAddressOrForwarder = BitConverter.ToUInt32(buffer, addressOffset + (i * sizeof(uint)))
                };
            }

            return functions;
        }

        public static void ReadExportAddressTable(this ExeStream stream) {
            var entry = stream.OptionalHeader.DataDirectories[H.IMAGE_DIRECTORY_ENTRY_EXPORT];
            var buffer = new byte[entry.Size];

            // Just try read the whole thing into memory. We need all parts anway.
            stream.SeekRva(entry.VirtualAddress);
            var baseRva = stream.Position; // Use to delta lookups easier.

            if (!stream.TryFullRead(buffer, 0, buffer.Length))
                throw new EndOfStreamException("Couldn't complete reads");

            var directory = WindowsStructConverter.ToExportDirectory(buffer, 0);
        }

        public static IEnumerable<ImportDescriptor> ReadImportDescriptors(this ExeStream stream) {
            var entry = stream.OptionalHeader.DataDirectories[H.IMAGE_DIRECTORY_ENTRY_IMPORT];
            var buffer = new byte[ImportDescriptor.Size];

            if (entry.Size == 0)
                yield break;

            var descriptorOffset = 0U;

            while(true) {
                stream.SeekRva(entry.VirtualAddress + descriptorOffset);
                stream.FullRead(buffer, 0, buffer.Length);
                var descriptor = WindowsStructConverter.ToImportDescriptor(buffer, 0);

                if (descriptor.Name == 0 && descriptor.FirstThunkRva == 0 && descriptor.OriginalFirstThunkRva == 0)
                    break;

                descriptorOffset += ImportDescriptor.Size;
                yield return descriptor;
            }
        }

        public static IEnumerable<ImportedFunction> ReadImportLocationTable(this ExeStream stream, ImportDescriptor descriptor) {
            // TODO: Share buffers
            var buffer = new byte[2048];

            // The thunk processing is the same for PE32 and PE32+, but the size of the thunks vary to support the tranlation
            var toThunk = stream.OptionalHeader.Magic == H.IMAGE_NT_OPTIONAL_HDR32_MAGIC ? ToThunk32 : ToThunk64;
            var ordinalFlag = 1UL << (stream.OptionalHeader.Magic == H.IMAGE_NT_OPTIONAL_HDR32_MAGIC ? 31 : 63);
            var thunkSize = stream.OptionalHeader.Magic == H.IMAGE_NT_OPTIONAL_HDR32_MAGIC ? sizeof(uint) : sizeof(ulong);
            var thunkOffset = 0;

            while (true) {
                stream.SeekRva(descriptor.OriginalFirstThunkRva + (uint)thunkOffset);
                stream.FullRead(buffer, 0, thunkSize);
                var thunk = toThunk(buffer, 0);

                if (thunk == 0)
                    break;

                // Determine if the import is by ordinal or by name/hint
                if ((thunk & ordinalFlag) != 0)
                    yield return new ImportedFunction { OrdinalOrHint = (ushort)(thunk & 0xFFFF) };
                else {
                    stream.SeekRva((uint)(thunk & 0x7FFFFFFF));
                    stream.FullRead(buffer, 0, sizeof(ushort));

                    yield return new ImportedFunction {
                        OrdinalOrHint = BitConverter.ToUInt16(buffer, 0),
                        Name = ReadAsciiString(stream, buffer, sizeof(ushort))
                    };
                }

                thunkOffset += thunkSize;
            }
        }

        public static void ReadImportAddressTable(this ExeStream stream) {
            var entry = stream.OptionalHeader.DataDirectories[H.IMAGE_DIRECTORY_ENTRY_IMPORT];

            stream.SeekRva(entry.VirtualAddress);
            DumpToFile(stream, "iat.bin", (int)entry.Size);
        }

        public static string ReadAsciiString(this Stream stream, byte[] buffer, int offset) {
            const int stringChunkSize = 64;

            var total = 0;
            var read = 0;
            var maxlen = buffer.Length - offset;

            while ((read = stream.Read(buffer, offset + total, Math.Min(stringChunkSize, maxlen - total))) > 0) {
                var terminator = Array.IndexOf<byte>(buffer, 0, offset + total, read);
                if (terminator >= 0)
                    return Encoding.ASCII.GetString(buffer, offset, terminator - offset);

                total += read;
            }

            if (total == maxlen)
                throw new ArgumentException("buffer was not large enough to read the entire string", nameof(buffer));

            throw new EndOfStreamException("could not finish reading the entire string");
        }

        private static void DumpToFile(ExeStream stream, string file, int len) {
            const int ChunkSize = 8192;

            using (var output = new FileStream(file, FileMode.Create, FileAccess.Write)) {
                var total = 0;
                var buffer = new byte[ChunkSize];

                while (total < len) {
                    if (!stream.TryFullRead(buffer, 0, Math.Min(len - total, ChunkSize), out var written))
                        throw new EndOfStreamException("Couldn't complete reads");

                    output.Write(buffer, 0, written);
                    total += written;
                }
            }
        }
    }
}
