using System;
using System.IO;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

namespace Linguini.Bench
{
    class BenchRunner
    {
#if DEBUG
        public static string BaseDir = Path.Combine(Path.GetFullPath("."), "..", "..", "..");
        static void Main(string[] args)
        {
            BenchmarkSwitcher.FromAssembly(typeof(BenchLinguiniParser).Assembly).Run(args, new DebugInProcessConfig());
        }
#else
        public static string BaseDir =
 Path.Combine(Path.GetFullPath("."), "..", "..", "..", "..", "..", "..", "..");

        static void Main(string[] args)
        { 
            BenchmarkRunner.Run<BenchLinguiniParser>();
        }
#endif
    }
}