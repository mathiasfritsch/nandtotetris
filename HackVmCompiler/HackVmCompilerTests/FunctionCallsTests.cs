using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackVmCompilerTests
{
    [TestClass]
    public class FunctionCallsTests
    {
        [TestMethod]
        public void SimpleFunction()
        {
            var testResult = TestHelper.CompileVmAndRunOnCpu(
    @"function SimpleFunction.test 2
push local 0
push local 1
add
not
push argument 0
add
push argument 1
sub
return",
    new Dictionary<int, int>
    {
                { 0,317},
                { 1,317},
                { 2,310},
                { 3,3000},
                { 4,4000},
                { 310,1234},
                { 311,37},
                { 312,1000},
                { 313,305},
                { 314,300},
                { 315,3010},
                { 316,4010}
    });
            var cpu = testResult.Cpu;
            Assert.AreEqual(311, cpu.RAM[0]);
            Assert.AreEqual(1196, cpu.RAM[310]);
            Assert.AreEqual(305, cpu.RAM[1]);
            Assert.AreEqual(300, cpu.RAM[2]);
            Assert.AreEqual(3010, cpu.RAM[3]);
            Assert.AreEqual(4010, cpu.RAM[4]);
        }

        [TestMethod]
        public void FunctionReturnsResult()
        {
            var testResult = TestHelper.CompileVmAndRunOnCpu(
        @"push constant 1
push constant 2
call SimpleFunction.test 2
goto PROGRAMM_END
function SimpleFunction.test 3
push argument 0
push argument 1
add
return
label PROGRAMM_END",
        new Dictionary<int, int>
        {
                { TestHelper.Stack,256},
                { TestHelper.Local,300},
                { TestHelper.Argument,400}
        });
            var cpu = testResult.Cpu;
            Assert.AreEqual(257, cpu.RAM[TestHelper.Stack]);
            Assert.AreEqual(3, cpu.Stack);
        }

        [TestMethod]
        public void CallFunctionSavesFrame()
        {
            var testResult = TestHelper.CompileVmAndRunOnCpu(
        @"push constant 1
push constant 2
call SimpleFunction.test 2
goto PROGRAMM_END
function SimpleFunction.test 0
push argument 0
push argument 1
add
return
label PROGRAMM_END",
        new Dictionary<int, int>
        {
                { TestHelper.Stack,256},
                { TestHelper.Local,300},
                { TestHelper.Argument,400},
                { TestHelper.This,2000},
                { TestHelper.That,3000}
        },
        stopAtVmLine: 5);
            var cpu = testResult.Cpu;

            var lineAfterCallFunction = cpu.PDB.Single(vm => vm.Value == 4);
            Assert.AreEqual(lineAfterCallFunction.Key - 2, cpu.RAM[258]);
            Assert.AreEqual(300, cpu.RAM[259]);
            Assert.AreEqual(400, cpu.RAM[260]);
            Assert.AreEqual(2000, cpu.RAM[261]);
            Assert.AreEqual(3000, cpu.RAM[262]);
            Assert.AreEqual(263, cpu.RAM[TestHelper.Stack]);
            Assert.AreEqual(256, cpu.RAM[TestHelper.Argument]);
            Assert.AreEqual(263, cpu.RAM[TestHelper.Local]);
        }
    }
}