﻿namespace Bhp.Compiler
{
    public class ConvOption
    {
        public bool useBrc8 = false;//將call 升級為callI'

        public bool useSysCallInteropHash = false;
        public static ConvOption Default
        {
            get
            {
                return new ConvOption();
            }
        }
    }
}
