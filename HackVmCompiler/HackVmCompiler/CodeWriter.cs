using System;
using System.IO;
using System.IO.Abstractions;

namespace HackVmCompiler
{
    public class CodeWriter : IDisposable
    {
        private StreamWriter fileStream;
        private readonly IFileSystem fileSystem;

        private const int memorySegmentLocalStart = 300;
        private const int memorySegmentArgumentStart = 400;
        private const int memorySegmentTempStart = 5;
        private const int memorySegmentThis = 3;
        private const int memorySegmentThat = 4;

        public void Close()
        {
            fileStream.Close();
        }

        public CodeWriter(IFileSystem fileSystem)
        {
            this.fileSystem = fileSystem;
        }

        public void SetFile(string fileName)
        {
            fileSystem.File.Delete(fileName);
            fileStream = fileSystem.FileInfo.FromFileName(fileName).CreateText();
        }

        public void WriteArithmetic(ArithmeticCommands command)

        {
            StackToD();
            DecreaseStackPointer();
            StackToM();
            if (command == ArithmeticCommands.add)
            {
                fileStream.WriteLine("M=M+D");
            }
            else if (command == ArithmeticCommands.sub)
            {
                fileStream.WriteLine("M=M-D");
            }
            else if (command == ArithmeticCommands.eq)
            {
                fileStream.WriteLine("D=D-M");
                DecreaseStackPointer();
                fileStream.WriteLine("@SETRESULT1");
                fileStream.WriteLine("D;JEQ");
                PushValueOnStack(0);
                IncreaseStackPointer();
                fileStream.WriteLine("@SETRESULTEND");
                fileStream.WriteLine("0;JMP");
                fileStream.WriteLine("(SETRESULT1)");
                PushValueOnStack(1);
                IncreaseStackPointer();
                fileStream.WriteLine("(SETRESULTEND)");
            }
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
                    fileStream.WriteLine($"@SP");
                    fileStream.WriteLine("A=M");
                    fileStream.WriteLine("M=D");
                }

                IncreaseStackPointer();
            }
            else if (command == CommandTypes.Pop)
            {
                PopValueFromStack(segment, index);
                DecreaseStackPointer();
            }
        }

        private void GetSegmentValueOnD(MemorySegments segment, int index)
        {
            int segmentPointer = 0;
            if (segment == MemorySegments.Local)
            {
                segmentPointer = memorySegmentLocalStart;
            }
            else if (segment == MemorySegments.Argument)
            {
                segmentPointer = memorySegmentArgumentStart;
            }
            if (segment == MemorySegments.This || segment == MemorySegments.That)
            {
                if (segment == MemorySegments.This)
                {
                    fileStream.WriteLine($"@{memorySegmentThis}");
                }
                else
                {
                    fileStream.WriteLine($"@{memorySegmentThat}");
                }
                fileStream.WriteLine($"D=M");
                fileStream.WriteLine($"@{index}");
                fileStream.WriteLine($"A=D+A");
            }
            else if (segment == MemorySegments.Temp)
            {
                segmentPointer = memorySegmentTempStart;
            }
            else if (segment == MemorySegments.Pointer)
            {
                segmentPointer = memorySegmentThis;
            }
            if (segment != MemorySegments.This && segment != MemorySegments.That)
            {
                fileStream.WriteLine($"@{segmentPointer + index}");
            }

            fileStream.WriteLine($"D=M");
        }

        private void IncreaseStackPointer()
        {
            fileStream.WriteLine($"@SP");
            fileStream.WriteLine("M=M+1");
        }

        private void DecreaseStackPointer()
        {
            fileStream.WriteLine($"@SP");
            fileStream.WriteLine("M=M-1");
        }

        private void PushValueOnStack(int valueToPush)
        {
            fileStream.WriteLine($"@{valueToPush}");
            fileStream.WriteLine("D=A");
            fileStream.WriteLine("@SP");
            fileStream.WriteLine("A=M");
            fileStream.WriteLine("M=D");
        }

        private void PopValueFromStack(MemorySegments segment, int index)
        {
            if (segment == MemorySegments.That || segment == MemorySegments.This)
            {
                if (segment == MemorySegments.This)
                    fileStream.WriteLine($"@{memorySegmentThis}");
                else
                    fileStream.WriteLine($"@{memorySegmentThat}");

                fileStream.WriteLine($"D=M");
                fileStream.WriteLine($"@{index}");
                fileStream.WriteLine($"D=D+A");
                fileStream.WriteLine($"@R13");
                fileStream.WriteLine($"M=D");
            }

            StackToD();
            int startSegment = 0;
            if (segment == MemorySegments.Local)
            {
                startSegment = memorySegmentLocalStart;
            }
            else if (segment == MemorySegments.Argument)
            {
                startSegment = memorySegmentArgumentStart;
            }
            else if (segment == MemorySegments.Temp)
            {
                startSegment = memorySegmentTempStart;
            }
            else if (segment == MemorySegments.Pointer)
            {
                startSegment = memorySegmentThis;
            }
            if (segment == MemorySegments.That || segment == MemorySegments.This)
            {
                fileStream.WriteLine($"@R13");
                fileStream.WriteLine($"A=M");
            }
            else
            {
                int itemAddress = startSegment + index;
                fileStream.WriteLine($"@{itemAddress}");
            }

            fileStream.WriteLine($"M=D");
        }

        private void StackToD()
        {
            StackToM();
            fileStream.WriteLine("D=M");
        }

        private void StackToM()
        {
            fileStream.WriteLine($"@SP");
            fileStream.WriteLine("A=M-1");
        }

        public void Dispose()
        {
            fileStream.Dispose();
        }
    }
}