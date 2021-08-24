using HackCPUMock;
using HackVmCompiler;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;

namespace HackVmCompilerTests
{
    public static class TestHelper
    {
        public static readonly int Stack = 0;
        public static readonly int Local = 1;
        public static readonly int Argument = 2;
        public static readonly int This = 3;
        public static readonly int That = 4;

        public static Cpu CompileVmAndRunOnCpu(string vmCode, Dictionary<int, int> initialData = null)
        {
            string vmFile = @"C:\somefile.vm";
            string asmFile = @"C:\loadconstant.asm";

            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
                {
                    {vmFile,
                    new MockFileData(vmCode) },
                });

            new Compiler(fileSystem, vmFile, asmFile).Run();
            var pdbContent = fileSystem.File.OpenText(@"C:\loadconstant.pdb").ReadToEnd();

            var cpu = new Cpu(fileSystem);
            if (initialData != null)
            {
                foreach (var address in initialData.Keys)
                {
                    cpu.RAM[address] = initialData[address];
                }
            }

            cpu.ReadAsm(asmFile);

            while (true)
            {
                if (!cpu.Step()) break;
            }

            return cpu;
        }
    }
}