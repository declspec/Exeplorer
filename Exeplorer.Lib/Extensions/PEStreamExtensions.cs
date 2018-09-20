using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Exeplorer.Lib.IO;
using Exeplorer.Lib.Windows;

namespace Exeplorer.Lib.Extensions {
    public static class PEStreamExtensions {
        public static string ReadString(this PEStream stream, byte[] buffer, int offset) {
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

            throw new EndOfStreamException("Failed to read from the underlying stream");
        }


        public static IEnumerable<ImageImportDescriptor> ReadImportDescriptors(this PEStream stream) {
            var entry = stream.OptionalHeader.DataDirectories[H.IMAGE_DIRECTORY_ENTRY_IMPORT];
            var buffer = new byte[ImageImportDescriptor.Size];

            if (entry.Size == 0)
                yield break;

            var descriptorOffset = 0U;

            while (true) {
                stream.SeekVirtualAddress(entry.VirtualAddress + descriptorOffset);
                stream.FullRead(buffer, 0, buffer.Length);

                var descriptor = WindowsStructConverter.ToImageImportDescriptor(buffer, 0);

                if (descriptor.Name == 0 && descriptor.FirstThunkRva == 0 && descriptor.OriginalFirstThunk == 0)
                    break;

                descriptorOffset += ImageImportDescriptor.Size;
                yield return descriptor;
            }
        }

        public static IEnumerable<uint> ReadImportLocationTable(this PEStream stream, ImageImportDescriptor descriptor) {
            // The thunk processing is the same for PE32 and PE32+, however the size of the thunks differ to support 64-bit addresses
            // when translating the ImportAddressTable (IAT). Luckily only 32-bits of the 64 are actually required in both
            // PE32 and PE32+ for the ImportLookupTable (ILT) so we can just normalize the data into a DWORD and avoid having to create
            // two *almost* identical methods.
            var is32Bit = stream.OptionalHeader.Magic == H.IMAGE_NT_OPTIONAL_HDR32_MAGIC;
            var thunkSize = (uint)(is32Bit ? sizeof(uint) : sizeof(ulong));
            var buffer = new byte[thunkSize];
            var thunkOffset = 0U;
            var thunk = 0U;

            while (true) {
                stream.SeekVirtualAddress(descriptor.OriginalFirstThunk + thunkOffset);
                stream.FullRead(buffer, 0, buffer.Length);

                if (is32Bit)
                    thunk = BitConverter.ToUInt32(buffer, 0);
                else {
                    var thunk64 = BitConverter.ToUInt64(buffer, 0);
                    thunk = (uint)((thunk64 & 0x7FFFFFFF) | ((thunk64 >> 32) & 0x80000000));
                }

                if (thunk == 0)
                    break;

                thunkOffset += thunkSize;
                yield return thunk;
            }
        }
    }
}
