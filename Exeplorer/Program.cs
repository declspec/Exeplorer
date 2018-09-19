using System;
using System.IO;
using System.Linq;
using Exeplorer.Extensions;
using Exeplorer.IO;

namespace Exeplorer
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var es = new ExeStream(new FileStream(@"C:\Windows\System32\KERNELBASE.dll", FileMode.Open, FileAccess.Read), AddressMode.File)) {
                var nameBuffer = new byte[4096];

                foreach(var descriptor in es.ReadImportDescriptors()) {
                    es.SeekRva(descriptor.Name);
                    Console.WriteLine(es.ReadAsciiString(nameBuffer, 0));

                    foreach(var fn in es.ReadImportLocationTable(descriptor).Where(fn => fn.Name == null))
                        Console.WriteLine("    {0}", fn.Name ?? $"#{fn.OrdinalOrHint}");
                }
            }

            Console.Read();
        }
    }
}
