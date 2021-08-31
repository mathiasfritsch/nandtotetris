using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

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
             }).Cpu;

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
             }).Cpu;

            Assert.AreEqual(257, cpu.RAM[TestHelper.Stack]);
            Assert.AreEqual(expextedResult, cpu.RAM[256]);
        }

        [TestMethod]
        public void FibonacciSeries()
        {
            var testResult = TestHelper.CompileVmAndRunOnCpu(
                @"push argument 1
pop pointer 1
push constant 0
pop that 0
push constant 1
pop that 1
push argument 0
push constant 2
sub
pop argument 0
label MAIN_LOOP_START
push argument 0
if-goto COMPUTE_ELEMENT
goto END_PROGRAM
label COMPUTE_ELEMENT
push that 0
push that 1
add
pop that 2
push pointer 1
push constant 1
add
pop pointer 1
push argument 0
push constant 1
sub
pop argument 0
goto MAIN_LOOP_START
label END_PROGRAM",
        new Dictionary<int, int>
             {
                { TestHelper.Stack,256},
                { TestHelper.Local,300},
                { TestHelper.Argument,400},
                { 400,6},
                { 401,3000}
             });
            var cpu = testResult.Cpu;
            Assert.AreEqual(0, cpu.RAM[3000]);
            Assert.AreEqual(1, cpu.RAM[3001]);
            Assert.AreEqual(1, cpu.RAM[3002]);
            Assert.AreEqual(2, cpu.RAM[3003]);
            Assert.AreEqual(3, cpu.RAM[3004]);
            Assert.AreEqual(5, cpu.RAM[3005]);
        }

        [TestMethod]
        public void BasicLoop()
        {
            var testResult = TestHelper.CompileVmAndRunOnCpu(
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
            var cpu = testResult.Cpu;

            Assert.AreEqual(257, cpu.RAM[TestHelper.Stack]);
            Assert.AreEqual(6, cpu.RAM[256]);
        }

        [TestMethod]
        public void FunctionReturnsResult()
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