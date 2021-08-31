using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Abstractions;
using System.Linq;

namespace HackCPUMock
{
    public class Cpu
    {
        private readonly IFileSystem fileSystem;
        private readonly Dictionary<string, int> symbolTable;
        public static readonly int TrueValue = -1;
        public static readonly int FalseValue = 0;
        private readonly Dictionary<int, int> pdb = new Dictionary<int, int>();
        private readonly int? stopAtVmLine;
        private readonly Dictionary<string, int> variableTable = new Dictionary<string, int>();

        private readonly Dictionary<string, int> fixedSymbols = new Dictionary<string, int>
        {
            { "@SP",0 },
            { "@LCL",1 },
            { "@ARG",2 },
            { "@THIS",3 },
            { "@THAT",4 }
        };

        private readonly Dictionary<string, int> fixedRegisters = new Dictionary<string, int>
        {
            { "@R13",13 },
            { "@R14",14 },
            { "@R15",15 }
        };

        public Dictionary<int, int> PDB
        {
            get
            {
                return pdb;
            }
        }

        public Cpu(IFileSystem fileSystem, int? stopAtVmLine = null)
        {
            this.fileSystem = fileSystem;
            this.symbolTable = new Dictionary<string, int>();
            this.stopAtVmLine = stopAtVmLine;
        }

        public void ReadPdb(string path)
        {
            using var fileStream = fileSystem.FileInfo.FromFileName(path).OpenRead();
            using var reader = new StreamReader(fileStream);
            while (reader.Peek() >= 0)
            {
                string[] pdbLineParts = reader.ReadLine().Split(' ');
                string[] vmParts = pdbLineParts[0].Split(':');
                string[] asmParts = pdbLineParts[1].Split(':');
                pdb.Add(int.Parse(asmParts[1]), int.Parse(vmParts[1]));
            }
        }

        public void ReadAsm(string path)
        {
            using var fileStream = fileSystem.FileInfo.FromFileName(path).OpenRead();
            using var reader = new StreamReader(fileStream);
            var pcIndex = 0;
            while (reader.Peek() >= 0)
            {
                string cmd = reader.ReadLine();
                string cmdWithoutComment;
                if (cmd.Contains("--"))
                {
                    cmdWithoutComment = cmd.Substring(0, cmd.IndexOf("--"));
                }
                else cmdWithoutComment = cmd;

                Programm[pcIndex] = cmdWithoutComment.Trim();
                pcIndex++;
            }

            var labelList = Programm
                    .Where(p => p != null && p.StartsWith("("))
                    .Select(p => p.TrimStart('(').TrimEnd(')'))
                    .ToArray();

            for (int instructionIndex = 0; instructionIndex < pcIndex; instructionIndex++)
            {
                var instruction = Programm[instructionIndex];

                if (Programm[instructionIndex].StartsWith("("))
                {
                    AddSymbol(instruction, instructionIndex);
                }
                if (instruction.StartsWith("@")
                    && !fixedSymbols.ContainsKey(instruction)
                    && !variableTable.ContainsKey(instruction)
                    && !fixedRegisters.ContainsKey(instruction)
                    && !labelList.Contains(instruction.TrimStart('@'))
                    && !instruction.TrimStart('@').All(Char.IsDigit))
                {
                    variableTable.Add(Programm[instructionIndex], variableTable.Count);
                }
            }
            for (int instructionIndex = 0; instructionIndex < pcIndex; instructionIndex++)
            {
                var instruction = Programm[instructionIndex];
                if (variableTable.ContainsKey(instruction))
                {
                    Programm[instructionIndex] = $"@{variableTable[instruction] + 16}";
                }
                else if (fixedSymbols.ContainsKey(instruction))
                {
                    Programm[instructionIndex] = $"@{fixedSymbols[instruction]}";
                }
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

        public int Stack
        {
            get
            {
                if (RAM[0] > 0) return RAM[RAM[0] - 1];
                return -1;
            }
        }

        public int M
        {
            get
            {
                return RAM[A];
            }
        }

        private void TraceStep(bool isInitialTrace = false)
        {
            Trace.WriteLine($"{(isInitialTrace ? "" : Instruction),-30} PC:{PC,5} A:{A,5} D:{D,5} M:{M,5} Stack:{Stack,5} SP:{RAM[0],5} LCL:{RAM[1],5} ");
        }

        public bool Step()
        {
            if (PC == 0) TraceStep(true);

            if (stopAtVmLine.HasValue)
            {
                int asmLine = PC + 1;
                if (pdb.ContainsKey(asmLine))
                {
                    int vmLine = pdb[asmLine];
                    if (vmLine == stopAtVmLine) return false;
                }
            }

            if (string.IsNullOrEmpty(Instruction)) return false;

            if (Instruction.StartsWith("@"))
            {
                HandleAInstruction();
            }
            else if (Instruction.StartsWith("("))
            {
                // lable found
            }
            else
            {
                HandleCInstruction();
            }

            TraceStep();
            PC++;
            return true;
        }

        private void AddSymbol(string instruction, int instructionIndex)
        {
            var symbol = instruction.TrimStart('(').TrimEnd(')');
            symbolTable.Add(symbol, instructionIndex);
        }

        private void HandleCInstruction()
        {
            if (Instruction.Contains("="))
            {
                HandleAssignment();
            }
            else if (Instruction.Contains(";"))
            {
                HandleJump();
            }
        }

        private void HandleJump()
        {
            string[] parts = Instruction.Split(";");
            var jumpValueContainer = parts[0];
            var jumpCompare = parts[1];
            bool doJump = false;
            int jumpValue = 0;
            if (jumpValueContainer == "A")
            {
                jumpValue = A;
            }
            else if (jumpValueContainer == "D")
            {
                jumpValue = D;
            }
            else if (jumpValueContainer == "M")
            {
                jumpValue = RAM[A];
            }

            if (jumpCompare == "JGT")
            {
                doJump = jumpValue > 0;
            }
            else if (jumpCompare == "JLT")
            {
                doJump = jumpValue < 0;
            }
            else if (jumpCompare == "JEQ")
            {
                doJump = jumpValue == 0;
            }
            else if (jumpCompare == "JNE")
            {
                doJump = jumpValue != 0;
            }
            else if (jumpCompare == "JMP")
            {
                doJump = true;
            }
            if (doJump) PC = A - 1;
        }

        private void HandleAssignment()
        {
            string[] parts = Instruction.Split("=");
            string target = parts[0];
            string source = parts[1];

            int valueToAssign = 0;
            if (source.Length == 3)
            {
                valueToAssign = GetValueFromRegisterOrMemory(source[0]);

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
                else if (operation == '&')
                {
                    valueToAssign &= secondValue;
                }
                else if (operation == '|')
                {
                    valueToAssign |= secondValue;
                }
            }
            else if (source.Length == 2)
            {
                char operation = source[0];
                int secondValue = GetValueFromRegisterOrMemory(source[1]);
                if (operation == '-')
                {
                    valueToAssign = -1 * secondValue;
                }
                else if (operation == '!')
                {
                    valueToAssign = ~secondValue;
                }
            }
            else
            {
                valueToAssign = GetValueFromRegisterOrMemory(source[0]);
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
            var instructionPayload = Instruction.TrimStart('@');

            if (instructionPayload == "SP")
            {
                A = 0;
            }
            else if (instructionPayload == "R13" || instructionPayload == "R14" || instructionPayload == "R15")
            {
                A = int.Parse(instructionPayload.TrimStart('R'));
            }
            else if (instructionPayload.All(char.IsDigit))
            {
                A = int.Parse(instructionPayload);
            }
            else
            {
                // handle lable ie @LOOP sets to where in which line
                // of the asm code (LOOP) appears
                A = symbolTable[instructionPayload];
            }
        }

        public void Reset()
        {
            PC = 0;
        }
    }
}