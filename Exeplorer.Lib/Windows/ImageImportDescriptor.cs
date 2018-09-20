namespace Exeplorer.Lib.Windows {
    public class ImageImportDescriptor {
        public const int Size = 20;

        public uint OriginalFirstThunk;
        public uint TimeDateStamp;
        public uint ForwarderChain;
        public uint Name;
        public uint FirstThunkRva;
    }
}
