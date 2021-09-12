using HackCPUMock;
using HackVmCompiler;
using System.Collections.Generic;
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

        public static CompilerTestResult CompileVmAndRunOnCpu(string vmCode,
            Dictionary<int, int> initialData = null,
            int? stopAtVmLine = null,
            string vmPath = null)
        {
            string asmFile = @"C:\loadconstant.asm";

            if (string.IsNullOrEmpty(vmPath))
            {
                vmPath = @"C:\somefile.vm";
            }

            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>{
                {vmPath,new MockFileData(vmCode)}
            });

            new Compiler(fileSystem, vmPath, asmFile, false).Run();

            var cpu = new Cpu(fileSystem, stopAtVmLine);
            if (initialData != null)
            {
                foreach (var address in initialData.Keys)
                {
                    cpu.RAM[address] = initialData[address];
                }
            }

            cpu.ReadAsm(asmFile);
            cpu.ReadPdb(asmFile.Replace(".asm", ".pdb"));

            int maxLoops = 10000;

            while (maxLoops-- > 0)
            {
                if (!cpu.Step()) break;
            }

            return new CompilerTestResult { Cpu = cpu, FileSystem = fileSystem };
        }
    }
}