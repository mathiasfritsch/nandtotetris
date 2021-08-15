using HackCPUMock;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;

namespace HackVmCompilerTests
{
    [TestClass]
    public class CpuTests
    {
        [TestMethod]
        public void AssignDTest()
        {
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
                {
                    { @"c:\cputest.asm", new MockFileData(@"@12
D=A") },
                });

            var cpu = new Cpu(fileSystem);
            cpu.ReadAsm(@"c:\cputest.asm");

            while (true)
            {
                if (!cpu.Step()) break;
            }

            Assert.AreEqual(12, cpu.D);
        }

        [TestMethod]
        public void AssignMTest()
        {
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
                {
                    { @"c:\cputest.asm", new MockFileData(@"@21
D=A
@5
M=D
") },
                });

            var cpu = new Cpu(fileSystem);
            cpu.ReadAsm(@"c:\cputest.asm");

            while (true)
            {
                if (!cpu.Step()) break;
            }

            Assert.AreEqual(21, cpu.RAM[5]);
        }

        [TestMethod]
        public void AddTest()
        {
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
                {
                    { @"c:\cputest.asm", new MockFileData(@"@21
D=A
@5
D=A+D
") },
                });

            var cpu = new Cpu(fileSystem);
            cpu.ReadAsm(@"c:\cputest.asm");

            while (true)
            {
                if (!cpu.Step()) break;
            }

            Assert.AreEqual(26, cpu.D);
        }
    }
}