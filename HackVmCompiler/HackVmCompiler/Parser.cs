using HackVmCompiler;
using System;
using System.IO;
using System.IO.Abstractions;

namespace HackVmCompiler
{
    public class Parser : IDisposable
    {
        private StreamReader file;
        private readonly IFileSystem fileSystem;

        public Parser(IFileSystem fileSystem)
        {
            this.fileSystem = fileSystem;
        }

        public void Advance()
        {
            string currentLine = file.ReadLine();
            if (currentLine.StartsWith("//") || currentLine.Length == 0)
            {
                CommandType = CommandTypes.Comment;
            }
            else
            {
                var token = currentLine.Split();

                var enumText = token[0].Remove('-');

                if (Enum.IsDefined(typeof(ArithmeticCommands), enumText))
                {
                    ArithmeticCommand = (ArithmeticCommands)Enum.Parse(typeof(ArithmeticCommands), enumText, true);
                    CommandType = CommandTypes.Arithmetic;
                }
                else CommandType = (CommandTypes)Enum.Parse(typeof(CommandTypes), enumText, true);

                if (token.Length > 1) Arg1 = token[1];
                if (token.Length > 2) Arg2 = token[2];

                if (CommandType == CommandTypes.Pop || CommandType == CommandTypes.Push)
                {
                    MemorySegment = (MemorySegments)Enum.Parse(typeof(MemorySegments), token[1], true);
                }
            }

            HasMoreCommands = !file.EndOfStream;
        }

        public CommandTypes CommandType
        {
            get; set;
        }

        public ArithmeticCommands ArithmeticCommand
        {
            get; set;
        }

        public MemorySegments MemorySegment
        {
            get; set;
        }

        public void SetFile(string fileName)
        {
            file = fileSystem.File.OpenText(fileName);
        }

        public void Dispose()
        {
            file.Dispose();
        }

        public bool HasMoreCommands
        {
            get; set;
        }

        public string Arg1
        {
            get; set;
        }

        public string Arg2
        {
            get; set;
        }
    }
}