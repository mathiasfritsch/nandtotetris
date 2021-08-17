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
        public void TestEqTrue(int constan1, int constant2, int result)
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
    }
}