using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using Exeplorer.Lib.Windows;

namespace Exeplorer.Lib.IO {
    public enum AddressMode {
        File,
        VirtualMemory
    }

    public class PEStream : Stream {
        private const string ErrorIncompleteRead = "Failed to read from the underlying stream";

        private readonly Stream _baseStream;
        private readonly long _baseOffset;
        private readonly AddressMode _addressMode;

        public ImageFileHeader FileHeader { get; }
        public ImageOptionalHeader OptionalHeader { get; }
        public IReadOnlyCollection<ImageSectionHeader> SectionHeaders { get; }

        public override bool CanRead => true;
        public override bool CanSeek => _baseStream.CanSeek;
        public override bool CanWrite => false;
        public override long Length => _baseStream.Length - _baseOffset;

        public override long Position {
            get => _baseStream.Position - _baseOffset;
            set => _baseStream.Position = value + _baseOffset;
        }

        public PEStream(Stream stream, AddressMode addressMode) {
            _baseStream = stream ?? throw new ArgumentNullException(nameof(stream));

            if (!_baseStream.CanRead)
                throw new ArgumentException("Must be a readable stream", nameof(stream));

            _baseOffset = stream.Position;
            _addressMode = addressMode;

            // Take a fairly generous buffer for reading the headers (may need to re-allocate if there a lot of section headers)
            var buffer = new byte[512];
            var lfanew = ReadLfanew(buffer);

            _baseStream.Seek(lfanew - H.IMAGE_SIZEOF_DOS_HEADER, SeekOrigin.Current);
            var ntHeader = ReadNtHeader(buffer);

            FileHeader = ntHeader.FileHeader;
            OptionalHeader = ntHeader.OptionalHeader;
            SectionHeaders = ReadSectionHeaders(FileHeader.NumberOfSections, ref buffer);
        }

        public override long Seek(long offset, SeekOrigin origin) {
            if (origin == SeekOrigin.Begin)
                offset += _baseOffset;

            return _baseStream.Seek(offset, origin);
        }

        public long SeekVirtualAddress(uint rva) {
            var section = GetEnclosingSectionHeader(rva);
            var delta = _addressMode == AddressMode.File ? (section.VirtualAddress - section.PointerToRawData) : 0;
            return Seek(rva - delta, SeekOrigin.Begin);
        }

        public ImageSectionHeader GetEnclosingSectionHeader(uint rva) {
            foreach (var section in SectionHeaders) {
                if (rva >= section.VirtualAddress && (rva < (section.VirtualAddress + (section.Misc.VirtualSize > 0 ? section.Misc.VirtualSize : section.SizeOfRawData))))
                    return section;
            }

            throw new EntryPointNotFoundException("Virtual Address is not part of any defined image section");
        }

        public override int Read(byte[] buffer, int offset, int count) {
            return _baseStream.Read(buffer, offset, count);
        }

        public void FullRead(byte[] buffer, int offset, int length) {
            if (!TryFullRead(buffer, offset, length))
                throw new EndOfStreamException(ErrorIncompleteRead);
        }

        public bool TryFullRead(byte[] buffer, int offset, int length) {
            return TryFullRead(buffer, offset, length, out var discard);
        }

        public bool TryFullRead(byte[] buffer, int offset, int length, out int total) {
            // Wrap partial reads
            int read;
            int originalOffset = offset;

            while (length > 0 && (read = _baseStream.Read(buffer, offset, length)) > 0) {
                length -= read;
                offset += read;
            }

            total = offset - originalOffset;
            return length == 0;
        }

        public override void Flush() {
            throw new NotSupportedException();
        }

        public override void SetLength(long value) {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count) {
            throw new NotSupportedException();
        }

        protected override void Dispose(bool disposing) {
            if (disposing)
                _baseStream.Dispose();
        }

        private uint ReadLfanew(byte[] buffer) {
            FullRead(buffer, 0, H.IMAGE_SIZEOF_DOS_HEADER);

            var magic = BitConverter.ToUInt16(buffer, 0);

            if (magic != H.IMAGE_DOS_SIGNATURE)
                throw new BadImageFormatException($"Invalid DOS signature found (0x{magic:X4}");

            return BitConverter.ToUInt32(buffer, H.IMAGE_SIZEOF_DOS_HEADER - sizeof(uint));
        }

        private ImageNtHeader ReadNtHeader(byte[] buffer) {
            // Do partial reads to account for variable image_opt_header sizes (x86/64)
            FullRead(buffer, 0, H.IMAGE_SIZEOF_FILE_HEADER + sizeof(uint) + sizeof(ushort));
            
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

            FullRead(buffer, magicOffset + sizeof(ushort), fileHeader.SizeOfOptionalHeader - sizeof(ushort));

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

            FullRead(buffer, 0, required);
            var headers = new ImageSectionHeader[numberOfSections];

            for (var i = 0; i < numberOfSections; ++i)
                headers[i] = WindowsStructConverter.ToImageSectionHeader(buffer, i * H.IMAGE_SIZEOF_SECTION_HEADER);

            return new ReadOnlyCollection<ImageSectionHeader>(headers);
        }
    }
}
