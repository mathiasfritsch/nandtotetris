using System;
using System.Collections.Generic;
using System.Text;

namespace HackVmCompiler
{
    public enum Pointer
    {
        Stack = 0,
        Local = 1,
        Argument = 2,
        This = 3,
        That = 4,
        Temp0 = 5,
        Temp1 = 6,
        Temp2 = 7,
        Temp3 = 8,
        Temp4 = 9,
        Temp5 = 10,
        Temp6 = 11,
        Temp7 = 12,
        R13 = 13,
        R14 = 14,
        R15 = 15
    }
}