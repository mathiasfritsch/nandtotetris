﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;

namespace HackCPUMock
{
    public class Cpu
    {
        private readonly IFileSystem _fileSystem;
        private Dictionary<string, int> _symbolTable;

        public Cpu(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
            RAM[0] = 256;
            _symbolTable = new Dictionary<string, int>();
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
            for (int instructionIndex = 0; instructionIndex < pcIndex; instructionIndex++)
            {
                var instruction = Programm[instructionIndex];

                if (Programm[instructionIndex].StartsWith("("))
                {
                    AddSymbol(instruction, instructionIndex);
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
                return RAM[RAM[0] - 1];
            }
        }

        public int M
        {
            get
            {
                return RAM[A];
            }
        }

        public bool Step()
        {
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
            PC++;
            return true;
        }

        private void AddSymbol(string instruction, int instructionIndex)
        {
            var symbol = instruction.TrimStart('(').TrimEnd(')');
            _symbolTable.Add(symbol, instructionIndex);
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
                else if (operation == '&')
                {
                    valueToAssign &= secondValue;
                }
                else if (operation == '|')
                {
                    valueToAssign |= secondValue;
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
                A = _symbolTable[instructionPayload];
            }
        }

        public void Reset()
        {
            PC = 0;
        }
    }
}