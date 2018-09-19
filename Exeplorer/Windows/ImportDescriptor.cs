namespace Exeplorer.Windows {
    public class ImportDescriptor {
        public const int Size = 20;

        public uint OriginalFirstThunkRva;
        public uint TimeDateStamp;
        public uint ForwarderChain;
        public uint Name;
        public uint FirstThunkRva;
    }
}
