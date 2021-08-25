using HackCPUMock;
using System.IO.Abstractions;

namespace HackVmCompilerTests
{
    public class CompilerTestResult
    {
        public Cpu Cpu { get; set; }
        public IFileSystem FileSystem { get; set; }
    }
}