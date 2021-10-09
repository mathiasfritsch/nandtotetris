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
        public void FunctionCalledTwice()
        {
            var testResult = TestHelper.CompileVmAndRunOnCpu(
        @"push constant 2
call SimpleFunction.test 1
push constant 3
call SimpleFunction.test 1
goto PROGRAMM_END
function SimpleFunction.test 0
push argument 0
push constant 5
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
            Assert.AreEqual(8, cpu.Stack);
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

        [TestMethod]
        public void SimpleCallNested()
        {
            var testResult = TestHelper.CompileVmAndRunOnCpu(
        @"function Sys.init 0
push constant 2
call Sys.add10Caller 1
label LOOP
goto LOOP
function Sys.add10Caller 0
push argument 0
call Sys.add10 1
return
function Sys.add10 0
push argument 0
push constant 10
add
return",
        new Dictionary<int, int>
        {
                { TestHelper.Stack,256},
                { TestHelper.Local,300},
                { TestHelper.Argument,400},
                { TestHelper.This,2000},
                { TestHelper.That,3000}
        },
        sourcePath: @"c:\sys.vm");
            var cpu = testResult.Cpu;
            Assert.AreEqual(12, cpu.Stack);
        }

        [TestMethod]
        public void NestedCall()
        {
            var testResult = TestHelper.CompileVmAndRunOnCpu(
        @"function Sys.init 0
push constant 4000	// test THIS and THAT context save
pop pointer 0
push constant 5000
pop pointer 1
call Sys.main 0
pop temp 1
label LOOP
goto LOOP
function Sys.main 5
push constant 4001
pop pointer 0
push constant 5001
pop pointer 1
push constant 200
pop local 1
push constant 40
pop local 2
push constant 6
pop local 3
push constant 123
call Sys.add12 1
pop temp 0
push local 0
push local 1
push local 2
push local 3
push local 4
add
add
add
add
return
function Sys.add12 0
push constant 4002
pop pointer 0
push constant 5002
pop pointer 1
push argument 0
push constant 12
add
return",
                new Dictionary<int, int>
                {
                        { TestHelper.Stack,261},
                        { TestHelper.Local,261},
                        { TestHelper.Argument,256},
                        { TestHelper.This,-3},
                        { TestHelper.That,-4},
                        { 5,-1},
                        { 6,-1},
                        { 256,1234},
                        { 257,-1},
                        { 258,-2},
                        { 259,-3},
                        { 260,-4},
                        { 261,-5},
                },
                sourcePath: @"c:\nestedcall.vm");
            var cpu = testResult.Cpu;
            Assert.AreEqual(261, cpu.RAM[TestHelper.Stack]);
            Assert.AreEqual(261, cpu.RAM[TestHelper.Local]);
            Assert.AreEqual(256, cpu.RAM[TestHelper.Argument]);
            Assert.AreEqual(4000, cpu.RAM[TestHelper.This]);
            Assert.AreEqual(5000, cpu.RAM[TestHelper.That]);
        }

        [TestMethod]
        [DataRow(1, 1)]
        [DataRow(2, 1)]
        [DataRow(3, 2)]
        [DataRow(4, 3)]
        [DataRow(5, 5)]
        [DataRow(6, 8)]
        public void Fibonacci_Test(int argument, int result)
        {
            Dictionary<string, string> vmFiles = new Dictionary<string, string>
            {
                {
"sys.vm",
@$"function Sys.init 0
push constant {argument}
call Main.fibonacci 1
label WHILE
goto WHILE" },
                {
"main.vm",
@"function Main.fibonacci 0
push argument 0
push constant 2
lt
if-goto IF_TRUE
goto IF_FALSE
label IF_TRUE
push argument 0
return
label IF_FALSE
push argument 0
push constant 2
sub
call Main.fibonacci 1
push argument 0
push constant 1
sub
call Main.fibonacci 1
add
return" }
            };

            var testResult = TestHelper.CompileVmAndRunOnCpu(
                    vmFiles: vmFiles,
                    new Dictionary<int, int>
                    {
                        { TestHelper.Stack,256},
                        { TestHelper.Local,300},
                        { TestHelper.Argument,400},
                        { TestHelper.This,2000},
                        { TestHelper.That,3000}
                    });

            var cpu = testResult.Cpu;
            Assert.AreEqual(result, cpu.Stack);
        }
    }
}