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

        [TestMethod]
        public void AndTest()
        {
            //1001
            //0011
            //0001

            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
                {
                    { @"c:\cputest.asm", new MockFileData(@"@5
D=A
@3
D=A&D
") },
                });

            var cpu = new Cpu(fileSystem);
            cpu.ReadAsm(@"c:\cputest.asm");

            while (true)
            {
                if (!cpu.Step()) break;
            }

            Assert.AreEqual(1, cpu.D);
        }

        [TestMethod]
        public void OrTest()
        {
            //1001
            //0010
            //1011

            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
                {
                    { @"c:\cputest.asm", new MockFileData(@"@5
D=A
@2
D=A|D
") },
                });

            var cpu = new Cpu(fileSystem);
            cpu.ReadAsm(@"c:\cputest.asm");

            while (true)
            {
                if (!cpu.Step()) break;
            }

            Assert.AreEqual(7, cpu.D);
        }

        [TestMethod]
        public void SymbolTest()
        {
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
                {
                    { @"c:\cputest.asm", new MockFileData(
@"@1
(START)
@2
@START
D=A
(OTHERLABLE)
@3
@4
@OTHERLABLE
") },
                });

            var cpu = new Cpu(fileSystem);
            cpu.ReadAsm(@"c:\cputest.asm");

            while (true)
            {
                if (!cpu.Step()) break;
            }

            Assert.AreEqual(5, cpu.A);
            Assert.AreEqual(1, cpu.D);
        }

        [TestMethod]
        public void JumpTest()
        {
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
                {
                    { @"c:\cputest.asm", new MockFileData(
@"@1
(START)
@2
@3
@END
0;JMP
@4
@5
@6
@7
(END)
") },
                });

            var cpu = new Cpu(fileSystem);
            cpu.ReadAsm(@"c:\cputest.asm");

            int loopCounter = 0;

            while (true)
            {
                var result = cpu.Step();
                if (!result || cpu.PC > 20) break;
                loopCounter++;
            }

            Assert.AreEqual(7, loopCounter);
        }
    }
}