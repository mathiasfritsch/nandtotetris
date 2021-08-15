using System;
using System.IO;
using System.IO.Abstractions;

namespace HackCPUMock
{
    public class Cpu
    {
        private readonly IFileSystem _fileSystem;

        public Cpu(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
            RAM[0] = 256;
        }

        public void ReadAsm(string path)
        {
            using var fileStream = _fileSystem.FileInfo.FromFileName(path).OpenRead();
            using var reader = new StreamReader(fileStream);
            var pcIndex = 0;
            while (reader.Peek() >= 0)
            {
                string cmd = reader.ReadLine();
                Programm[pcIndex] = cmd;
                pcIndex++;
            }
        }

        public int[] RAM = new int[64000];

        public int A = 0;

        public int D = 0;

        public string[] Programm = new string[1000];

        public int PC = 0;

        public string Instruction
        {
            get
            {
                return Programm[PC];
            }
        }

        public bool Step()
        {
            if (string.IsNullOrEmpty(Instruction)) return false;

            if (Instruction.StartsWith("@"))
            {
                HandleAInstruction();
            }
            else
            {
                HandleCInstruction();
            }
            PC++;
            return true;
        }

        private void HandleCInstruction()
        {
            if (Instruction.Contains("="))
            {
                HandleAssignment();
            }
        }

        private void HandleAssignment()
        {
            string[] parts = Instruction.Split("=");
            string target = parts[0];
            string source = parts[1];

            int valueToAssign = GetValueFromRegisterOrMemory(source[0]);
            if (source.Length == 3)
            {
                char operation = source[1];
                int secondValue = GetValueFromRegisterOrMemory(source[2]);
                if (operation == '+')
                {
                    valueToAssign += secondValue;
                }
                else if (operation == '-')
                {
                    valueToAssign -= secondValue;
                }
            }

            if (target == "D") D = valueToAssign;
            else if (target == "A") A = valueToAssign;
            else if (target == "M") RAM[A] = valueToAssign;
            else throw new ArgumentException($"Target {target} is not defined");
        }

        private int GetValueFromRegisterOrMemory(char source)
        {
            if (source == '0') return 0;
            else if (source == '1') return 1;
            else if (source == 'D') return D;
            else if (source == 'A') return A;
            else if (source == 'M') return RAM[A];
            else throw new ArgumentException($"Source {source} is not defined");
        }

        private void HandleAInstruction()
        {
            if (Instruction == ("@SP"))
            {
                A = 0;
            }
            else if (Instruction.StartsWith("@R"))
            {
                A = int.Parse(Instruction.Replace("@R", ""));
            }
            else
            {
                A = int.Parse(Instruction.TrimStart('@'));
            }
        }

        public void Reset()
        {
            PC = 0;
        }
    }
}