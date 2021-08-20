using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackVmCompilerTests
{
    [TestClass]
    public class ProgrammFlowTests
    {
        [TestMethod]
        public void BasicLoop()
        {
            var cpu = TestHelper.CompileVmAndRunOnCpu(
             @"push constant 0
pop local 0
label LOOP_START
push argument 0
push local 0
add
pop local 0
push argument 0
push constant 1
sub
pop argument 0
push argument 0
if-goto LOOP_START
push local 0",
             new Dictionary<int, int>
             {
                { TestHelper.Stack,256},
                { TestHelper.Local,300},
                { TestHelper.Argument,400},
                { 400,3}
             });

            Assert.AreEqual(257, cpu.RAM[TestHelper.Stack]);
            Assert.AreEqual(6, cpu.RAM[257]);
        }
    }
}