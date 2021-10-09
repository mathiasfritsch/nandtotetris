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

        public static CompilerTestResult CompileVmAndRunOnCpu(Dictionary<string, string> vmFiles,
         Dictionary<int, int> initialData = null,
         int? stopAtVmLine = null)
        {
            string sourcePath = @"C:\somefolder";

            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>());

            foreach (var file in vmFiles)
            {
                fileSystem.AddFile($"{sourcePath}/{file.Key}", new MockFileData(file.Value));
            }

            return CompileVmAndRunOnCpu(initialData, stopAtVmLine, sourcePath, fileSystem);
        }

        public static CompilerTestResult CompileVmAndRunOnCpu(string vmCode,
            Dictionary<int, int> initialData = null,
            int? stopAtVmLine = null,
            string sourcePath = null)
        {
            if (string.IsNullOrEmpty(sourcePath))
            {
                sourcePath = @"C:\somefile.vm";
            }

            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>{
                {sourcePath,new MockFileData(vmCode)}
            });

            return CompileVmAndRunOnCpu(initialData, stopAtVmLine, sourcePath, fileSystem);
        }

        private static CompilerTestResult CompileVmAndRunOnCpu(
            Dictionary<int, int> initialData,
            int? stopAtVmLine,
            string sourcePath,
            MockFileSystem fileSystem)
        {
            string asmFile = @"C:\somefile.asm";

            new Compiler(fileSystem, sourcePath, asmFile, false).Run();

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