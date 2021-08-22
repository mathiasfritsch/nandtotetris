using HackCPUMock;
using HackVmCompiler;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;

namespace HackVmCompilerTests
{
    public static class TestHelper
    {
        public static readonly int Stack = 0;
        public static readonly int Local = 1;
        public static readonly int Argument = 2;
        public static readonly int This = 3;
        public static readonly int That = 4;

        public static Cpu CompileVmAndRunOnCpu(string vmCode, Dictionary<int, int> initialData = null)
        {
            string vmFile = @"C:\somefile.vm";
            string asmFile = @"C:\loadconstant.asm";

            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
                {
                    {vmFile,
                    new MockFileData(vmCode) },
                });

            var parser = new Parser(fileSystem);
            parser.SetFile(vmFile);

            var codeWriter = new CodeWriter(fileSystem);
            codeWriter.SetFile(asmFile);
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
                else if (cmd == CommandTypes.Label)
                {
                    codeWriter.WriteLabel(parser.Arg1);
                }
                else if (cmd == CommandTypes.Goto)
                {
                    codeWriter.WriteGoto(parser.Arg1);
                }
                else if (cmd == CommandTypes.IfGoto)
                {
                    codeWriter.WriteIf(parser.Arg1);
                }
                if (!parser.HasMoreCommands) break;
            }
            codeWriter.Dispose();

            var cpu = new Cpu(fileSystem);
            if (initialData != null)
            {
                foreach (var address in initialData.Keys)
                {
                    cpu.RAM[address] = initialData[address];
                }
            }

            cpu.ReadAsm(asmFile);

            while (true)
            {
                if (!cpu.Step()) break;
            }
            codeWriter.Close();
            return cpu;
        }
    }
}