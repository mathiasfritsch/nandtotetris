using HackCPUMock;
using HackVmCompiler;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;

namespace HackVmCompilerTests
{
    [TestClass]
    public class MemoryAccessTests
    {
        private static Cpu CompileVmAndRunOnCpu(string vmCode)
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
            cpu.ReadAsm(asmFile);

            while (true)
            {
                if (!cpu.Step()) break;
            }
            codeWriter.Close();
            return cpu;
        }

        [TestMethod]
        public void PushTest()
        {
            var cpu = CompileVmAndRunOnCpu(@"push constant 5");
            Assert.AreEqual(5, cpu.RAM[256]);
        }

        [TestMethod]
        public void BasicTest()
        {
            var cpu = CompileVmAndRunOnCpu(
@"push constant 10
pop local 0
push constant 21
push constant 22
pop argument 2
pop argument 1
push constant 36
pop this 6
push constant 42
push constant 45
pop that 5
pop that 2
push constant 510
pop temp 6
push local 0
push that 5
add
push argument 1
sub
push this 6
push this 6
add
sub
push temp 6
add");
            Assert.AreEqual(472, cpu.RAM[256]);
        }
    }
}