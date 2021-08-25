using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace HackVmCompilerTests
{
    [TestClass]
    public class StackArithmeticTests
    {
        public static readonly int TrueValue = -1;
        public static readonly int FalseValue = 0;

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
    }).Cpu;

            Assert.AreEqual(15, cpu.Stack);
        }

        [TestMethod]
        public void StackTest()
        {
            var cpu = TestHelper.CompileVmAndRunOnCpu(
    @"push constant 17
push constant 17
eq
push constant 17
push constant 16
eq
push constant 16
push constant 17
eq
push constant 892
push constant 891
lt
push constant 891
push constant 892
lt
push constant 891
push constant 891
lt
push constant 32767
push constant 32766
gt
push constant 32766
push constant 32767
gt
push constant 32766
push constant 32766
gt
push constant 57
push constant 31
push constant 53
add
push constant 112
sub
neg
and
push constant 82
or
not",
    new Dictionary<int, int>
    {
                       {0,256}
    }).Cpu;

            Assert.AreEqual(266, cpu.RAM[0]);
            Assert.AreEqual(-1, cpu.RAM[256]);
            Assert.AreEqual(0, cpu.RAM[257]);
            Assert.AreEqual(0, cpu.RAM[258]);
            Assert.AreEqual(0, cpu.RAM[259]);
            Assert.AreEqual(-1, cpu.RAM[260]);
            Assert.AreEqual(0, cpu.RAM[261]);
            Assert.AreEqual(-1, cpu.RAM[262]);
            Assert.AreEqual(0, cpu.RAM[263]);
            Assert.AreEqual(0, cpu.RAM[264]);
            Assert.AreEqual(-91, cpu.RAM[265]);
        }

        [TestMethod]
        [DataRow(16, 16, -1)]
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
    }).Cpu;

            Assert.AreEqual(result, cpu.Stack);
        }

        [TestMethod]
        [DataRow(16, 16, 0)]
        [DataRow(12, 16, -1)]
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
            }).Cpu;
            Assert.AreEqual(result, cpu.Stack);
        }

        [TestMethod]
        [DataRow(16, 16, 0)]
        [DataRow(12, 16, 0)]
        [DataRow(16, 12, -1)]
        public void TestGt(int constan1, int constant2, int result)
        {
            var cpu = TestHelper.CompileVmAndRunOnCpu(
    $@"push constant {constan1}
push constant {constant2}
gt",
            new Dictionary<int, int>
            {
                               {0,256}
            }).Cpu;
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
            }).Cpu;
            Assert.AreEqual(-1, cpu.Stack);
        }

        [TestMethod]
        public void TestNeg()
        {
            var cpu = TestHelper.CompileVmAndRunOnCpu(
    $@"push constant 3
neg",
            new Dictionary<int, int>
            {
                               {0,256}
            }).Cpu;
            Assert.AreEqual(-3, cpu.Stack);
        }

        [TestMethod]
        public void TestAnd()
        {
            var cpu = TestHelper.CompileVmAndRunOnCpu(
    $@"push constant 5
push constant 6
and",
            new Dictionary<int, int>
            {
                               {0,256}
            }).Cpu;
            Assert.AreEqual(4, cpu.Stack);
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
    }).Cpu;

            Assert.AreEqual(FalseValue, cpu.RAM[3031]);
            Assert.AreEqual(TrueValue, cpu.RAM[3032]);
            Assert.AreEqual(FalseValue, cpu.RAM[3033]);
        }
    }
}