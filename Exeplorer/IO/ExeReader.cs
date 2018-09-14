using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Exeplorer.Windows;

namespace Exeplorer.IO {
    public class ExeReader : IDisposable {
        private const string ErrorIncompleteRead = "Failed to read from the underlying stream";

        private readonly Stream _stream;
        private readonly long _base;
        private readonly AddressMode _addressMode;

        public ImageFileHeader FileHeader;
        public ImageOptionalHeader OptionalHeader;
        public IReadOnlyCollection<ImageSectionHeader> SectionHeaders { get; }

        public ExeReader(Stream stream, AddressMode addressMode) {
            _stream = stream ?? throw new ArgumentNullException(nameof(stream));
            _base = stream.Position;
            _addressMode = addressMode;

            // Take a fairly generous buffer for reading the headers (may need to re-allocate if there a lot of section headers)
            var buffer = new byte[512];
            var dosHeader = ReadDosHeader(buffer);

            _stream.Seek(dosHeader.Lfanew - H.IMAGE_SIZEOF_DOS_HEADER, SeekOrigin.Current);
            var ntHeader = ReadNtHeader(buffer);

            FileHeader = ntHeader.FileHeader;
            OptionalHeader = ntHeader.OptionalHeader;
            SectionHeaders = ReadSectionHeaders(FileHeader.NumberOfSections, ref buffer);
        }



        private long SeekToVirtualAddress(uint rva) {
            foreach(var section in SectionHeaders) {
                if (rva >= section.VirtualAddress && (rva < (section.VirtualAddress + (section.Misc.VirtualSize > 0 ? section.Misc.VirtualSize : section.SizeOfRawData)))) {
                    var delta = _addressMode == AddressMode.File ? (section.VirtualAddress - section.PointerToRawData) : 0;
                    return _stream.Seek(_base + (rva - delta), SeekOrigin.Begin);
                }
            }

            throw new EntryPointNotFoundException("Virtual Address is not part of any defined image section");
        }

        private ImageDosHeader ReadDosHeader(byte[] buffer) {
            if (!TryRead(buffer, 0, H.IMAGE_SIZEOF_DOS_HEADER))
                throw new EndOfStreamException(ErrorIncompleteRead);

            var dosHeader = WindowsStructConverter.ToImageDosHeader(buffer, 0);

            if (dosHeader.Magic != H.IMAGE_DOS_SIGNATURE)
                throw new BadImageFormatException($"Invalid DOS signature found (0x{dosHeader.Magic:X4}");

            return dosHeader;
        }

        private ImageNtHeader ReadNtHeader(byte[] buffer) {
            // Do partial reads to account for variable image_opt_header sizes (x86/64)
            if (!TryRead(buffer, 0, H.IMAGE_SIZEOF_FILE_HEADER + sizeof(uint) + sizeof(ushort)))
                throw new EndOfStreamException(ErrorIncompleteRead);

            // Pre-read the opt-header's magic value to ensure the size matches what we expect
            var magicOffset = H.IMAGE_SIZEOF_FILE_HEADER + sizeof(uint);
            var signature = BitConverter.ToUInt32(buffer, 0);
            var magic = BitConverter.ToUInt16(buffer, magicOffset);

            if (signature != H.IMAGE_NT_SIGNATURE)
                throw new BadImageFormatException($"Invalid NT signature found (0x{signature:X8}");

            if (magic != H.IMAGE_NT_OPTIONAL_HDR32_MAGIC && magic != H.IMAGE_NT_OPTIONAL_HDR64_MAGIC)
                throw new BadImageFormatException($"Invalid optional header magic value found (0x{magic:X4})");

            var fileHeader = WindowsStructConverter.ToImageFileHeader(buffer, sizeof(uint));
            var expectedSize = magic == H.IMAGE_NT_OPTIONAL_HDR32_MAGIC ? ImageOptionalHeader32.Size : ImageOptionalHeader64.Size;

            if (fileHeader.SizeOfOptionalHeader != expectedSize)
                throw new BadImageFormatException("Invalid optional header size encountered");

            if (!TryRead(buffer, magicOffset + sizeof(ushort), fileHeader.SizeOfOptionalHeader - sizeof(ushort)))
                throw new EndOfStreamException(ErrorIncompleteRead);

            return new ImageNtHeader() {
                Signature = signature,
                FileHeader = fileHeader,
                OptionalHeader = WindowsStructConverter.ToImageOptionalHeader(buffer, magicOffset)
            };
        }

        private IReadOnlyCollection<ImageSectionHeader> ReadSectionHeaders(int numberOfSections, ref byte[] buffer) {
            var required = numberOfSections * H.IMAGE_SIZEOF_SECTION_HEADER;

            // TODO: Do some validation on numberOfSections, this could easily be a target of stupidly large allocations
            if (required > buffer.Length)
                buffer = new byte[required];

            if (!TryRead(buffer, 0, required))
                throw new EndOfStreamException(ErrorIncompleteRead);

            var headers = new ImageSectionHeader[numberOfSections];
            for (var i = 0; i < numberOfSections; ++i)
                headers[i] = WindowsStructConverter.ToImageSectionHeader(buffer, i * H.IMAGE_SIZEOF_SECTION_HEADER);

            return new ReadOnlyCollection<ImageSectionHeader>(headers);
        }

        private bool TryRead(byte[] buffer, int offset, int length) {
            // Wrap partial reads
            int read;

            while(length > 0 && (read = _stream.Read(buffer, offset, length)) > 0) {
                length -= read;
                offset += read;
            }

            return length == 0;
        }

        public void Dispose() {
            
        }
    }
}
