﻿using Mono.Cecil;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Bhp.Compiler
{
    public static class Helper
    {
        // System.Void Bhp.Compiler.MSIL.TestClasses.Contract_syscall::.cctor()
        private readonly static Regex _regex_cctor = new Regex(@".*\:\:\.cctor\(\)");
        // System.Void Bhp.Compiler.MSIL.TestClasses.Contract_syscall::.ctor(System.Int32)
        private readonly static Regex _regex_ctor = new Regex(@".*\:\:\.ctor\(.*\)");

        public static bool Is_cctor(this MethodDefinition method)
        {
            return method.IsConstructor && _regex_cctor.IsMatch(method.FullName);
        }

        public static bool Is_ctor(this MethodDefinition method)
        {
            return method.IsConstructor && _regex_ctor.IsMatch(method.FullName);
        }

        public static uint ToInteropMethodHash(this string method)
        {
            return ToInteropMethodHash(Encoding.ASCII.GetBytes(method));
        }

        public static uint ToInteropMethodHash(this byte[] method)
        {
            using (SHA256 sha = SHA256.Create())
            {
                return BitConverter.ToUInt32(sha.ComputeHash(method), 0);
            }
        }
        public static byte[] HexString2Bytes(string str)
        {
            if (str.IndexOf("0x") == 0)
                str = str.Substring(2);
            byte[] outd = new byte[str.Length / 2];
            for (var i = 0; i < str.Length / 2; i++)
            {
                outd[i] = byte.Parse(str.Substring(i * 2, 2), System.Globalization.NumberStyles.HexNumber);
            }
            return outd;
        }
        public static byte[] OpDataToBytes(string opdata)
        {
            try  // convert hex string to byte[]
            {
                return HexString2Bytes(opdata);
            }
            catch (Exception e)
            {
                return System.Text.Encoding.UTF8.GetBytes(opdata);
            }
        }
    }
}
