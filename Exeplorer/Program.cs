using System;
using System.IO;
using Exeplorer.Lib.Extensions;
using Exeplorer.Lib.IO;
using Exeplorer.Lib.Windows;

namespace Exeplorer
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var es = new PEStream(new FileStream(@"C:\Windows\System32\KERNELBASE.dll", FileMode.Open, FileAccess.Read), AddressMode.File)) {
                var nameBuffer = new byte[4096];

                foreach(var descriptor in es.ReadImportDescriptors()) {
                    es.SeekVirtualAddress(descriptor.Name);
                    Console.WriteLine(es.ReadString(nameBuffer, 0));

                    foreach (var thunk in es.ReadImportLocationTable(descriptor)) {
                        if ((thunk & H.IMAGE_ORDINAL_FLAG32) != 0)
                            Console.WriteLine("    #{0}", thunk & 0xFFFF);
                        else {
                            es.SeekVirtualAddress(thunk + 2);
                            Console.WriteLine("    {0}", es.ReadString(nameBuffer, 0));
                        }
                    }
                }
            }

            Console.Read();
        }
    }
}
