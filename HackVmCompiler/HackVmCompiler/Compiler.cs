using System.IO;
using System.IO.Abstractions;

namespace HackVmCompiler
{
    public class Compiler
    {
        private readonly IFileSystem fileSystem;
        private readonly string sourcePath;

        private readonly string targetPath;
        private readonly bool writeComments = false;

        public Compiler(IFileSystem fileSystem, string sourcePath, string targetPath, bool writeComments)

        {
            this.fileSystem = fileSystem;
            this.sourcePath = sourcePath;
            this.targetPath = targetPath;
            this.writeComments = writeComments;
        }

        public void Run()
        {
            var parser = new Parser(fileSystem);
            var codeWriter = new CodeWriter(fileSystem);

            parser.SetFileOrFolder(sourcePath);
            codeWriter.SetFile(targetPath);
            var pdbFile = targetPath.Replace(".asm", ".pdb");

            fileSystem.File.Delete(pdbFile);
            var fileInfo = fileSystem.FileInfo.FromFileName(pdbFile);

            using StreamWriter fileStreamPdb = fileInfo.CreateText();
            int lineVm = 0;
            if (sourcePath.EndsWith("sys.vm", comparisonType: System.StringComparison.InvariantCultureIgnoreCase))
            {
                codeWriter.WriteBootstrap();
            }
            while (true)
            {
                fileStreamPdb.WriteLine($@"lineVm:{PrettyNumber(lineVm + 1)} lineAsm:{PrettyNumber(codeWriter.AsmLineIndex + 1)}");

                parser.Advance();
                if (writeComments)
                {
                    codeWriter.CurrentVmLine = parser.CurrentLine;
                }

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
                else if (cmd == CommandTypes.Call)
                {
                    codeWriter.WriteCallFunction(parser.Arg1, parser.Arg2);
                }
                else if (cmd == CommandTypes.Function)
                {
                    codeWriter.WriteFunction(parser.Arg1, int.Parse(parser.Arg2));
                }
                else if (cmd == CommandTypes.Return)
                {
                    codeWriter.WriteReturn();
                }

                if (!parser.HasMoreCommands) break;
                lineVm++;
            }
            codeWriter.Close();
        }

        private string PrettyNumber(int number) => number.ToString("000");
    }
}