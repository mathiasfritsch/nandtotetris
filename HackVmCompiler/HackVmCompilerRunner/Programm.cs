using HackVmCompiler;
using System.IO.Abstractions;

namespace HackVmCompilerRunner
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            var sourcePath = args.Length > 0 ? args[0] : @"C:\TEMP\SimpleFunctionDebug2\SimpleFunction.vm";
            var targetPath = args.Length > 1 ? args[1] : @"C:\TEMP\SimpleFunctionDebug2\SimpleFunction.asm";

            Compiler compiler = new Compiler(new FileSystem(), sourcePath, targetPath, true);
            compiler.Run();
        }
    }
}