using System.IO;
using System.IO.Abstractions;

namespace HackVmCompiler
{
    public class Compiler
    {
        private readonly IFileSystem fileSystem;
        private readonly string sourcePath;

        private readonly string targetPath;

        public Compiler(IFileSystem fileSystem, string sourcePath, string targetPath)

        {
            this.fileSystem = fileSystem;
            this.sourcePath = sourcePath;
            this.targetPath = targetPath;
        }

        public void Run()
        {
            var parser = new Parser(fileSystem);
            var codeWriter = new CodeWriter(fileSystem);

            parser.SetFile(sourcePath);
            codeWriter.SetFile(targetPath);
            var pdbFile = targetPath.Replace(".asm", ".pdb");

            fileSystem.File.Delete(pdbFile);
            using StreamWriter fileStreamPdb = fileSystem.FileInfo.FromFileName(pdbFile).CreateText();
            int lineVm = 0;

            while (true)
            {
                fileStreamPdb.WriteLine($@"lineVm:{PrettyNumber(lineVm)} lineAsm:{PrettyNumber(codeWriter.AsmLineIndex)}");
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
                lineVm++;
            }
            codeWriter.Close();
        }

        private string PrettyNumber(int number) => number.ToString("000");
    }
}