using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WordsLive.Core.Songs;
using System.IO;
using System.Diagnostics;
using WordsLive.Core.Data;

namespace SongTester
{
    class Program
    {
        static void Main(string[] args)
        {
            var dir = new DirectoryInfo(@"C:\Users\Patrick\Documents\Powerpraise-Dateien\Songs");
            int i = 0;
            Stopwatch sw = new Stopwatch();
            sw.Start();
            foreach (var f in dir.GetFiles())
            {
                Song s = new Song(f.FullName);
                i++;
                Console.WriteLine("[OK] " +s.SongTitle);
            }
            sw.Stop();
            Console.WriteLine();
            Console.WriteLine("Loaded " + i + " songs in " + sw.Elapsed.ToString());
            Console.Read();
        }
    }
}
