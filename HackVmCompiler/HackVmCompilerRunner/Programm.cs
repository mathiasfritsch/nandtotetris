using HackVmCompiler;
using System.IO.Abstractions;

namespace HackVmCompilerRunner
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            var sourcePath = args.Length > 0 ? args[0] : @"C:\projects\nand2tetris\projects\08\FunctionCalls\SimpleFunctionDebug\SimpleFunction.vm";
            var targetPath = args.Length > 1 ? args[1] : @"C:\projects\nand2tetris\projects\08\FunctionCalls\SimpleFunctionDebug\SimpleFunction.asm";

            Compiler compiler = new Compiler(new FileSystem(), sourcePath, targetPath);
            compiler.Run();
        }
    }
}