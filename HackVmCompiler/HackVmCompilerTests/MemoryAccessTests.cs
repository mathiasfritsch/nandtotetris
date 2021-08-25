using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace HackVmCompilerTests
{
    [TestClass]
    public class MemoryAccessTests
    {
        [TestMethod]
        public void PushTest()
        {
            var cpu = TestHelper.CompileVmAndRunOnCpu(@"push constant 5").Cpu;
            Assert.AreEqual(5, cpu.RAM[256]);
        }

        [TestMethod]
        public void BasicTest()
        {
            var cpu = TestHelper.CompileVmAndRunOnCpu(
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
add",
                new Dictionary<int, int>
                {
                    {0,256},
                    {1,300},
                    {2,400},
                    {3,3000 },
                    {4,3010 }
                }).Cpu;

            Assert.AreEqual(472, cpu.RAM[256]);
            Assert.AreEqual(10, cpu.RAM[300]);
            Assert.AreEqual(21, cpu.RAM[401]);
            Assert.AreEqual(22, cpu.RAM[402]);
            Assert.AreEqual(36, cpu.RAM[3006]);
            Assert.AreEqual(42, cpu.RAM[3012]);
            Assert.AreEqual(45, cpu.RAM[3015]);
            Assert.AreEqual(510, cpu.RAM[11]);
        }

        [TestMethod]
        public void PopThis()
        {
            var cpu = TestHelper.CompileVmAndRunOnCpu(
                @"pop this 6",
                new Dictionary<int, int>
                {
                    { 3,3030},
                    { 0,257},
                    { 256,241}
                }).Cpu;

            Assert.AreEqual(241, cpu.RAM[3036]);
        }

        [TestMethod]
        public void PushThis()
        {
            var cpu = TestHelper.CompileVmAndRunOnCpu(
                @"push this 5",
                new Dictionary<int, int>
                {
                    { 3,3030},
                    { 5,3030},
                    { 3035,305}
                }).Cpu;

            Assert.AreEqual(305, cpu.RAM[256]);
            Assert.AreEqual(257, cpu.RAM[0]);
        }
    }
}