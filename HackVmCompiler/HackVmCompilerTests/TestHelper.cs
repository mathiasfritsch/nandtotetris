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