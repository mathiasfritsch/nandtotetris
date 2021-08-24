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
        public void GotoTest()
        {
            var cpu = TestHelper.CompileVmAndRunOnCpu(
             @"goto FIRSTLABEL
push constant 1
goto ENDLABEL
label FIRSTLABEL
push constant 2
label ENDLABEL",

             new Dictionary<int, int>
             {
                { TestHelper.Stack,256}
             });

            Assert.AreEqual(257, cpu.RAM[TestHelper.Stack]);
            Assert.AreEqual(2, cpu.RAM[256]);
        }

        [TestMethod]
        [DataRow(0, 1)]
        [DataRow(1, 2)]
        public void IfGotoTest(int conditionValue, int expextedResult)
        {
            var cpu = TestHelper.CompileVmAndRunOnCpu(
             $@"push constant {conditionValue}
if-goto FIRSTLABEL
push constant 1
goto ENDLABEL
label FIRSTLABEL
push constant 2
label ENDLABEL",

             new Dictionary<int, int>
             {
                { TestHelper.Stack,256}
             });

            Assert.AreEqual(257, cpu.RAM[TestHelper.Stack]);
            Assert.AreEqual(expextedResult, cpu.RAM[256]);
        }

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