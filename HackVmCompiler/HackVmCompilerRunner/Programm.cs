using HackVmCompiler;
using System.IO.Abstractions;

namespace HackVmCompilerRunner
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            var sourcePath = args.Length > 0 ? args[0] : @"C:\projects\nand2tetris\projects\07\MemoryAccess\PointerTestDebug\PointerTest.vm";
            var targetPath = args.Length > 1 ? args[1] : @"C:\projects\nand2tetris\projects\07\MemoryAccess\PointerTestDebug\PointerTest.asm";

            var parser = new Parser(new FileSystem());
            var codeWriter = new CodeWriter(new FileSystem());

            parser.SetFile(sourcePath);

            codeWriter.SetFile(targetPath);
            while (true)
            {
                parser.Advance();
                var cmd = parser.CommandType;
                if (cmd == CommandTypes.Arithmetic)
                {
                    codeWriter.WriteArithmetic(parser.ArithmeticCommand);
                }
                else if (cmd == CommandTypes.Pop || cmd == CommandTypes.Push)
                {
                    codeWriter.WritePushPop(cmd, parser.MemorySegment, int.Parse(parser.Arg2));
                }
                else if (cmd == CommandTypes.Goto)
                {
                    codeWriter.WriteGoto(parser.Arg1);
                }
                else if (cmd == CommandTypes.IfGoto)
                {
                    codeWriter.WriteIf(parser.Arg1);
                }
                else if (cmd == CommandTypes.Label)
                {
                    codeWriter.WriteLabel(parser.Arg1);
                }
                if (!parser.HasMoreCommands) break;
            }
            codeWriter.Close();
        }
    }
}