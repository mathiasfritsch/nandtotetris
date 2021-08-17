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
        public void TestEqTrue()
        {
            var cpu = TestHelper.CompileVmAndRunOnCpu(
    @"push constant 17
push constant 17
eq",
    new Dictionary<int, int>
    {
                       {0,256}
    });

            Assert.AreEqual(1, cpu.Stack);
        }
    }
}