﻿namespace Bhp.Compiler.ASM
{
    internal enum InstructionName : byte
    {
        PUSH,
        NOP,
        JMP,
        JMPIF,
        JMPIFNOT,
        CALL,
        RET,
        APPCALL,
        SYSCALL,
        TAILCALL,
        DUPFROMALTSTACK,
        TOALTSTACK,
        FROMALTSTACK,
        XDROP,
        XSWAP,
        XTUCK,
        DEPTH,
        DROP,
        DUP,
        NIP,
        OVER,
        PICK,
        ROLL,
        ROT,
        SWAP,
        TUCK,
        CAT,
        SUBSTR,
        LEFT,
        RIGHT,
        SIZE,
        INVERT,
        AND,
        OR,
        XOR,
        EQUAL,
        INC,
        DEC,
        SIGN,
        NEGATE,
        ABS,
        NOT,
        NZ,
        ADD,
        SUB,
        MUL,
        DIV,
        MOD,
        SHL,
        SHR,
        BOOLAND,
        BOOLOR,
        NUMEQUAL,
        NUMNOTEQUAL,
        LT,
        GT,
        LTE,
        GTE,
        MIN,
        MAX,
        WITHIN,
        SHA1,
        SHA256,
        HASH160,
        HASH256,
        CHECKSIG,
        CHECKMULTISIG,
        ARRAYSIZE,
        PACK,
        UNPACK,
        PICKITEM,
        SETITEM,
        NEWARRAY,
        NEWSTRUCT,
        THROW,
        THROWIFNOT,
    }
}
