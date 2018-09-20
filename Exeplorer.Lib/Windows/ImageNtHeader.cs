namespace Exeplorer.Lib.Windows {
    public class ImageNtHeader {
        public uint Signature;
        public ImageFileHeader FileHeader;
        public ImageOptionalHeader OptionalHeader;
    }
}
