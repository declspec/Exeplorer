using System;
using System.IO;
using Exeplorer.Extensions;
using Exeplorer.IO;

namespace Exeplorer
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var es = new ExeStream(new FileStream(@"C:\Windows\System32\kernel32.dll", FileMode.Open, FileAccess.Read), AddressMode.File)) {
                es.ReadExportAddressTable();
            }

            Console.Read();
        }
    }
}
