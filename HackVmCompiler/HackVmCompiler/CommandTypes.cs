using System;
using System.Collections.Generic;
using System.Text;

namespace HackVmCompiler
{
    public enum CommandTypes
    {
        Arithmetic,
        Push,
        Pop,
        Label,
        Goto,
        If,
        Function,
        Return,
        Call,
        Comment,
        IfGoto
    }
}