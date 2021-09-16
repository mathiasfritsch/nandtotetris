using System.IO;
using System.IO.Abstractions;
using System.Linq;

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
            var pdbFileInfo = fileSystem.FileInfo.FromFileName(pdbFile);

            using StreamWriter fileStreamPdb = pdbFileInfo.CreateText();
            int lineVm = 0;

            if (parser.IsFolder)
            {
                var initFile = parser.Files.SingleOrDefault(f => f.EndsWith("sys.vm", System.StringComparison.InvariantCultureIgnoreCase));
                if (initFile != null)
                {
                    parser.SetFileOrFolder(initFile);
                    HandleInputFile(
                        parser,
                        codeWriter,
                        fileStreamPdb,
                        lineVm,
                        writeBootstrap: true);
                }
                foreach (var file in parser.Files.Where(f => !f.EndsWith("sys.vm", System.StringComparison.InvariantCultureIgnoreCase)))
                {
                    parser.SetFileOrFolder(file);
                    lineVm = 0;
                    HandleInputFile(parser, codeWriter, fileStreamPdb, lineVm);
                }
            }
            else
            {
                HandleInputFile(parser, codeWriter, fileStreamPdb, lineVm);
            }

            codeWriter.Close();
        }

        private void HandleInputFile(Parser parser,
            CodeWriter codeWriter,
            StreamWriter fileStreamPdb,
            int lineVm,
            bool writeBootstrap = false)
        {
            if (writeBootstrap)
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
        }

        private string PrettyNumber(int number) => number.ToString("000");
    }
}