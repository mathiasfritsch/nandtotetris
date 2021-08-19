using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace HackVmCompilerTests
{
    [TestClass]
    public class StackArithmeticTests
    {
        [TestMethod]
        public void SimpleAddTest()
        {
            var cpu = TestHelper.CompileVmAndRunOnCpu(
    @"push constant 7
push constant 8
add",
    new Dictionary<int, int>
    {
                       {0,256}
    });

            Assert.AreEqual(15, cpu.Stack);
        }

        [TestMethod]
        [DataRow(16, 16, 1)]
        [DataRow(12, 16, 0)]
        public void TestEq(int constan1, int constant2, int result)
        {
            var cpu = TestHelper.CompileVmAndRunOnCpu(
    $@"push constant {constan1}
push constant {constant2}
eq",
    new Dictionary<int, int>
    {
                       {0,256}
    });

            Assert.AreEqual(result, cpu.Stack);
        }

        [TestMethod]
        [DataRow(16, 16, 0)]
        [DataRow(12, 16, 1)]
        [DataRow(16, 12, 0)]
        public void TestLt(int constan1, int constant2, int result)
        {
            var cpu = TestHelper.CompileVmAndRunOnCpu(
    $@"push constant {constan1}
push constant {constant2}
lt",
            new Dictionary<int, int>
            {
                               {0,256}
            });
            Assert.AreEqual(result, cpu.Stack);
        }

        [TestMethod]
        [DataRow(16, 16, 0)]
        [DataRow(12, 16, 0)]
        [DataRow(16, 12, 1)]
        public void TestGt(int constan1, int constant2, int result)
        {
            var cpu = TestHelper.CompileVmAndRunOnCpu(
    $@"push constant {constan1}
push constant {constant2}
gt",
            new Dictionary<int, int>
            {
                               {0,256}
            });
            Assert.AreEqual(result, cpu.Stack);
        }

        [TestMethod]
        public void TestNot()
        {
            var cpu = TestHelper.CompileVmAndRunOnCpu(
    $@"push constant 0
not",
            new Dictionary<int, int>
            {
                               {0,256}
            });
            Assert.AreEqual(-1, cpu.Stack);
        }

        [TestMethod]
        public void TestMulipleEq()
        {
            var cpu = TestHelper.CompileVmAndRunOnCpu(
    $@"push constant 4
push constant 3
eq
pop this 1
push constant 4
push constant 4
eq
pop this 2
push constant 7
push constant 8
eq
pop this 3
",
    new Dictionary<int, int>
    {
                    { 3,3030},
                     {0,256}
    });

            Assert.AreEqual(0, cpu.RAM[3031]);
            Assert.AreEqual(1, cpu.RAM[3032]);
            Assert.AreEqual(0, cpu.RAM[3033]);
        }
    }
}