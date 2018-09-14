namespace Exeplorer.Windows {
    public struct ImageNtHeader {
        public uint Signature;
        public ImageFileHeader FileHeader;
        public ImageOptionalHeader OptionalHeader;
    }
}
