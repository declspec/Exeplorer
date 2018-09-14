using System;
using System.IO;
using System.Linq;
using Exeplorer.IO;
using Exeplorer.Windows;

namespace Exeplorer.Extensions {
    public static class ExeStreamExtensions {
        public static void ReadExportAddressTable(this ExeStream stream) {
            var entry = stream.OptionalHeader.DataDirectories[H.IMAGE_DIRECTORY_ENTRY_EXPORT];
            var buffer = new byte[entry.Size];

            // Just try read the whole thing into memory. We need all parts anway.
            stream.SeekRva(entry.VirtualAddress);
            var baseRva = stream.Position; // Use to delta lookups easier.

            if (!stream.TryRead(buffer, 0, buffer.Length))
                throw new EndOfStreamException("Couldn't complete reads");

            var directory = WindowsStructConverter.ToImageExportDirectory(buffer, 0);



            // For the ENT, try read it all at once
            stream.SeekRva()

            Console.WriteLine("0x{0:X8}", directory.AddressOfFunctions);
            Console.WriteLine("0x{0:X8}", directory.AddressOfNames);
            Console.WriteLine("0x{0:X8}", directory.AddressOfNameOrdinals);
            Console.WriteLine("0x{0:X8}", directory.Name);

            stream.SeekRva(directory.AddressOfFunctions);
            DumpToFile(stream, "functions.dat", (int)(directory.AddressOfNames - directory.AddressOfFunctions));
            stream.SeekRva(directory.AddressOfNames);
            DumpToFile(stream, "name-rvas.dat", (int)(directory.AddressOfNameOrdinals - directory.AddressOfNames));
            stream.SeekRva(directory.AddressOfNameOrdinals);
            DumpToFile(stream, "ordinals.dat", (int)(directory.Name - directory.AddressOfNameOrdinals));
            stream.SeekRva(directory.Name);
            DumpToFile(stream, "names.dat", (int)((entry.VirtualAddress + entry.Size) - directory.Name));
            Console.WriteLine();
        }

        private static uint GetExportNameTableSize(ImageDataDirectory entry, ImageExportDirectory directory) {
            var next = new[] { directory.AddressOfFunctions, directory.AddressOfNames, directory.AddressOfNameOrdinals, directory.AddressOfFunctions, entry.VirtualAddress + entry.Size }
                .Where(i => i > directory.Name)
                .Min();

            return next - directory.Name;
        }

        private static void DumpToFile(ExeStream stream, string file, int len) {
            const int ChunkSize = 8192;

            using (var output = new FileStream(file, FileMode.Create, FileAccess.Write)) {
                var total = 0;
                var buffer = new byte[ChunkSize];

                while (total < len) {
                    if (!stream.TryRead(buffer, 0, Math.Min(len - total, ChunkSize), out var written))
                        throw new EndOfStreamException("Couldn't complete reads");

                    output.Write(buffer, 0, written);
                    total += written;
                }
            }
        }
    }
}
