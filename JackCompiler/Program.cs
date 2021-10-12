using JackCompilationEngine;
using System;
using System.IO.Abstractions;

namespace JackCompiler
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var sourcePath = args.Length > 0 ? args[0] : @"C:\\projects\\nand2tetris\\projects\\10\\ArrayTest";
            var targetPath = args.Length > 1 ? args[1] : @"C:\TEMP\FibonacciElementDebug\FibonacciElement.asm";

            Tokenizer tokenizer = new Tokenizer(new FileSystem(), sourcePath);
            tokenizer.WriteXml(targetPath);
        }
    }
}