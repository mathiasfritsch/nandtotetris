using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;

namespace HackVmCompiler
{
    public class CodeWriter : IDisposable
    {
        private StreamWriter fileStream;
        private readonly IFileSystem fileSystem;
        private readonly string methodName = "main";
        private const int memorySegmentLcl = 1;
        private const int memorySegmentTempStart = 5;
        private const int memorySegmentArg = 2;
        private const int memorySegmentThis = 3;
        private const int memorySegmentThat = 4;
        public static readonly int TrueValue = -1;
        public static readonly int FalseValue = 0;
        private int branchingCounter = 0;
        private bool vmLineWrittenAsCommand = false;
        private string currentVmLine;
        public int AsmLineIndex { private set; get; }
        public int FunctionCallCount { private set; get; }

        public void Close()
        {
            fileStream.Close();
        }

        public string CurrentVmLine
        {
            get
            {
                return currentVmLine;
            }
            set
            {
                currentVmLine = value;
                vmLineWrittenAsCommand = false;
            }
        }

        public void WriteBootstrap()
        {
            WriteAsmCommand("@256");
            WriteAsmCommand("D=A");
            WriteAsmCommand("@SP");
            WriteAsmCommand("M=D");
        }

        public CodeWriter(IFileSystem fileSystem)
        {
            this.fileSystem = fileSystem;
            FunctionCallCount = 0;
            AsmLineIndex = 0;
        }

        public void SetFile(string fileName)
        {
            fileSystem.File.Delete(fileName);
            fileStream = fileSystem.FileInfo.FromFileName(fileName).CreateText();
        }

        public void WriteArithmetic(ArithmeticCommands command)

        {
            StackToD();
            if (command == ArithmeticCommands.not)
            {
                WriteAsmCommand("M=!D");
                return;
            }
            else if (command == ArithmeticCommands.neg)
            {
                WriteAsmCommand("M=-D");
                return;
            }
            DecreaseStackPointer();
            StackToM();
            if (command == ArithmeticCommands.add)
            {
                WriteAsmCommand("M=M+D");
            }
            else if (command == ArithmeticCommands.sub)
            {
                WriteAsmCommand("M=M-D");
            }
            else if (command == ArithmeticCommands.or)
            {
                WriteAsmCommand("M=M|D");
            }
            else if (command == ArithmeticCommands.and)
            {
                WriteAsmCommand("M=M&D");
            }
            else if (command == ArithmeticCommands.eq
                || command == ArithmeticCommands.gt
                || command == ArithmeticCommands.lt)
            {
                WriteAsmCommand("D=M-D");
                DecreaseStackPointer();
                WriteAsmCommand($"@SETRESULTTRUE{branchingCounter}");
                if (command == ArithmeticCommands.eq)
                {
                    WriteAsmCommand("D;JEQ");
                }
                else if (command == ArithmeticCommands.gt)
                {
                    WriteAsmCommand("D;JGT");
                }
                else if (command == ArithmeticCommands.lt)
                {
                    WriteAsmCommand("D;JLT");
                }
                PushValueOnStack(FalseValue);
                WriteAsmCommand($"@SETRESULTEND{branchingCounter}");
                WriteAsmCommand("0;JMP");
                WriteAsmCommand($"(SETRESULTTRUE{branchingCounter})");
                PushValueOnStack(TrueValue);
                WriteAsmCommand($"(SETRESULTEND{branchingCounter})");
                branchingCounter++;
            }
        }

        public void WriteFunction(string functionName, int lclVariables)
        {
            string thisFunction = $"{functionName}";
            WriteLabel(thisFunction);
            for (int i = 0; i < lclVariables; i++)
            {
                SetLclToZero();
            }
        }

        public void WriteCallFunction(string functionName, string args)
        {
            WriteAsmCommand($"// call function {functionName}");

            var caller = $"{FunctionCallCount}.backlable";
            FunctionCallCount++;

            string calledFunction = $"{functionName}";
            WriteAsmCommand($"@{caller}");
            PushAOnStack();
            PushSegmentAddressOnStack(MemorySegments.Local);
            PushSegmentAddressOnStack(MemorySegments.Argument);
            PushSegmentAddressOnStack(MemorySegments.This);
            PushSegmentAddressOnStack(MemorySegments.That);
            RepositionArg(int.Parse(args));
            RepositionLocal();
            WriteGoto(calledFunction);
            WriteAsmCommand($"// caller  {caller}");
            WriteLabel(caller);
        }

        public void WriteReturn()
        {
            //FRAME = LCL
            WriteAsmCommand("// frame=lcl");
            WriteAsmCommand("@LCL");
            WriteAsmCommand("D=M");
            WriteAsmCommand("@frame");
            WriteAsmCommand("M=D");

            //Return = FRAME-5
            WriteAsmCommand("// return = frame - 5");
            WriteAsmCommand("@5");
            WriteAsmCommand("D=D-A");
            WriteAsmCommand("A=D");
            WriteAsmCommand("D=M");
            WriteAsmCommand("@return");
            WriteAsmCommand("M=D");

            // return value on argument 0
            WriteAsmCommand("// argument0  = returnvalue");
            PopValueFromStack(MemorySegments.Argument, 0);

            // SP = arg + 1
            WriteAsmCommand("// sp = arg + 1");
            WriteAsmCommand("@ARG");
            WriteAsmCommand("D=M+1");
            WriteAsmCommand("@SP");
            WriteAsmCommand("M=D");

            //that = FRAME-1
            WriteAsmCommand("// that = frame - 1");
            WriteAsmCommand("@frame");
            WriteAsmCommand("A=M-1");
            WriteAsmCommand("D=M");
            WriteAsmCommand("@THAT");
            WriteAsmCommand("M=D");

            ////this = FRAME-2
            WriteAsmCommand("// this = frame - 2");
            WriteAsmCommand("@frame");
            WriteAsmCommand("D=M");
            WriteAsmCommand("@2");
            WriteAsmCommand("D=D-A");
            WriteAsmCommand("A=D");
            WriteAsmCommand("D=M");
            WriteAsmCommand("@THIS");
            WriteAsmCommand("M=D");

            ////arg = FRAME-3
            WriteAsmCommand("// arg = FRAME-3");
            WriteAsmCommand("@frame");
            WriteAsmCommand("D=M");
            WriteAsmCommand("@3");
            WriteAsmCommand("D=D-A");
            WriteAsmCommand("A=D");
            WriteAsmCommand("D=M");
            WriteAsmCommand("@ARG");
            WriteAsmCommand("M=D");

            ////local = FRAME-4
            WriteAsmCommand("// local = FRAME-4");
            WriteAsmCommand("@frame");
            WriteAsmCommand("D=M");
            WriteAsmCommand("@4");
            WriteAsmCommand("D=D-A");
            WriteAsmCommand("A=D");
            WriteAsmCommand("D=M");
            WriteAsmCommand("@LCL");
            WriteAsmCommand("M=D");

            //Return
            WriteAsmCommand("// return");
            WriteAsmCommand("@return");
            WriteAsmCommand("A=M");
            WriteAsmCommand("0;JMP");
        }

        private void SetLclToZero()
        {
            WriteAsmCommand("@SP");
            WriteAsmCommand("A=M");
            WriteAsmCommand("M=0");
            WriteAsmCommand("@SP");
            WriteAsmCommand("M=M+1");
        }

        private void RepositionLocal()
        {
            WriteAsmCommand($"@SP");
            WriteAsmCommand($"D=M");
            WriteAsmCommand($"@1");
            WriteAsmCommand($"M=D");
        }

        private void RepositionArg(int argsCount)
        {
            // arg = sp - 5 - argscount
            WriteAsmCommand($"@SP");
            WriteAsmCommand($"D=M");
            WriteAsmCommand($"@5");
            WriteAsmCommand($"D=D-A");
            WriteAsmCommand($"@{argsCount}");
            WriteAsmCommand($"D=D-A");
            WriteAsmCommand($"@2");
            WriteAsmCommand($"M=D");
        }

        private void PushSegmentAddressOnStack(MemorySegments segment)
        {
            if (segment == MemorySegments.Local)
            {
                WriteAsmCommand("@1");
            }
            else if (segment == MemorySegments.Argument)
            {
                WriteAsmCommand("@2");
            }
            else if (segment == MemorySegments.This)
            {
                WriteAsmCommand("@3");
            }
            else if (segment == MemorySegments.That)
            {
                WriteAsmCommand("@4");
            }
            WriteAsmCommand("D=M");
            PushDOnStack();
        }

        public void WriteLabel(string label)
        {
            WriteAsmCommand($"({label})");
        }

        public void WriteGoto(string label)
        {
            WriteAsmCommand($"@{label}");
            WriteAsmCommand("0;JMP");
        }

        public void WriteIf(string label)
        {
            StackToD();
            DecreaseStackPointer();
            WriteAsmCommand($"@{label}");
            WriteAsmCommand($"D;JNE");
        }

        public void WritePushPop(CommandTypes command, MemorySegments segment, int index)
        {
            if (command == CommandTypes.Push)
            {
                if (segment == MemorySegments.Constant)
                {
                    PushValueOnStack(index);
                }
                else
                {
                    GetSegmentValueOnD(segment, index);
                    WriteAsmCommand($"@SP");
                    WriteAsmCommand("A=M");
                    WriteAsmCommand("M=D");
                    IncreaseStackPointer();
                }
            }
            else if (command == CommandTypes.Pop)
            {
                PopValueFromStack(segment, index);
            }
        }

        private void GetSegmentValueOnD(MemorySegments segment, int index)
        {
            int segmentPointer = 0;

            if (segment == MemorySegments.This ||
                segment == MemorySegments.That ||
                segment == MemorySegments.Local ||
                segment == MemorySegments.Argument)
            {
                if (segment == MemorySegments.This)
                {
                    WriteAsmCommand($"@{memorySegmentThis}");
                }
                else if (segment == MemorySegments.That)
                {
                    WriteAsmCommand($"@{memorySegmentThat}");
                }
                else if (segment == MemorySegments.Local)
                {
                    WriteAsmCommand($"@{memorySegmentLcl}");
                }
                else
                {
                    WriteAsmCommand($"@{memorySegmentArg}");
                }

                WriteAsmCommand($"D=M");
                WriteAsmCommand($"@{index}");
                WriteAsmCommand($"A=D+A");
            }
            else if (segment == MemorySegments.Temp)
            {
                segmentPointer = memorySegmentTempStart;
            }
            else if (segment == MemorySegments.Pointer)
            {
                segmentPointer = memorySegmentThis;
            }
            if (segment != MemorySegments.This
                && segment != MemorySegments.That
                && segment != MemorySegments.Argument
                && segment != MemorySegments.Local)
            {
                WriteAsmCommand($"@{segmentPointer + index}");
            }

            WriteAsmCommand($"D=M");
        }

        private void IncreaseStackPointer()
        {
            WriteAsmCommand($"@SP");
            WriteAsmCommand("M=M+1");
        }

        private void DecreaseStackPointer()
        {
            WriteAsmCommand($"@SP");
            WriteAsmCommand("M=M-1");
        }

        private void PushAOnStack()
        {
            WriteAsmCommand("D=A");
            WriteAsmCommand("@SP");
            WriteAsmCommand("A=M");
            WriteAsmCommand("M=D");
            IncreaseStackPointer();
        }

        private void PushDOnStack()
        {
            WriteAsmCommand("@SP");
            WriteAsmCommand("A=M");
            WriteAsmCommand("M=D");
            IncreaseStackPointer();
        }

        private void PushValueOnStack(int valueToPush)
        {
            if (valueToPush >= -1 && valueToPush <= 1)
            {
                WriteAsmCommand($"D={valueToPush}");
            }
            else
            {
                WriteAsmCommand($"@{valueToPush}");
                WriteAsmCommand("D=A");
            }

            WriteAsmCommand("@SP");
            WriteAsmCommand("A=M");
            WriteAsmCommand("M=D");
            IncreaseStackPointer();
        }

        private void PopValueFromStack(MemorySegments segment, int index)
        {
            if (segment == MemorySegments.That
                || segment == MemorySegments.This
                || segment == MemorySegments.Local
                 || segment == MemorySegments.Argument
                )
            {
                if (segment == MemorySegments.This)
                    WriteAsmCommand($"@{memorySegmentThis}");
                else if (segment == MemorySegments.That)
                    WriteAsmCommand($"@{memorySegmentThat}");
                else if (segment == MemorySegments.Local)
                    WriteAsmCommand($"@{memorySegmentLcl}");
                else if (segment == MemorySegments.Argument)
                    WriteAsmCommand($"@{memorySegmentArg}");

                WriteAsmCommand($"D=M");
                WriteAsmCommand($"@{index}");
                WriteAsmCommand($"D=D+A");
                WriteAsmCommand($"@R13");
                WriteAsmCommand($"M=D");
            }

            StackToD();
            int startSegment = 0;

            if (segment == MemorySegments.Temp)
            {
                startSegment = memorySegmentTempStart;
            }
            else if (segment == MemorySegments.Pointer)
            {
                startSegment = memorySegmentThis;
            }
            if (segment == MemorySegments.That
                || segment == MemorySegments.This
                  || segment == MemorySegments.Local
                    || segment == MemorySegments.Argument)
            {
                WriteAsmCommand($"@R13");
                WriteAsmCommand($"A=M");
            }
            else
            {
                int itemAddress = startSegment + index;
                WriteAsmCommand($"@{itemAddress}");
            }

            WriteAsmCommand($"M=D");
            DecreaseStackPointer();
        }

        private void StackToD()
        {
            StackToM();
            WriteAsmCommand("D=M");
        }

        private void StackToM()
        {
            WriteAsmCommand($"@SP");
            WriteAsmCommand("A=M-1");
        }

        public void Dispose()
        {
            fileStream.Dispose();
        }

        private void WriteAsmCommand(string cmd)
        {
            if (!vmLineWrittenAsCommand && !string.IsNullOrEmpty(currentVmLine))
            {
                fileStream.WriteLine($"// {currentVmLine}");
                AsmLineIndex++;
                vmLineWrittenAsCommand = true;
            }
            fileStream.WriteLine(cmd);
            AsmLineIndex++;
        }
    }
}