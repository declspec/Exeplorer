namespace Exeplorer.Windows {
    public class NtHeader {
        public uint Signature;
        public FileHeader FileHeader;
        public OptionalHeader OptionalHeader;
    }
}
