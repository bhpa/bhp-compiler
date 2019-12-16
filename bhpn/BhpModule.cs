using Mono.Cecil;
using Bhp.Compiler.MSIL;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Bhp.Compiler
{
    public class BhpModule
    {
        public BhpModule(ILogger logger) { }

        public string mainMethod;
        public ConvOption option;
        public List<CustomAttribute> attributes = new List<CustomAttribute>();
        public Dictionary<string, BhpMethod> mapMethods = new Dictionary<string, BhpMethod>();
        public Dictionary<string, BhpEvent> mapEvents = new Dictionary<string, BhpEvent>();
        public Dictionary<string, BhpField> mapFields = new Dictionary<string, BhpField>();
        public Dictionary<string, object> staticfieldsWithConstValue = new Dictionary<string, object>();
        public List<ILMethod> staticfieldsCctor = new List<ILMethod>();
        public SortedDictionary<int, BhpCode> total_Codes = new SortedDictionary<int, BhpCode>();

        public byte[] Build()
        {
            List<byte> bytes = new List<byte>();
            foreach (var c in this.total_Codes.Values)
            {
                bytes.Add((byte)c.code);
                if (c.bytes != null)
                    for (var i = 0; i < c.bytes.Length; i++)
                    {
                        bytes.Add(c.bytes[i]);
                    }
            }
            return bytes.ToArray();
            //将body链接，生成this.code       byte[]
            //并计算 this.codehash            byte[]
        } //public Dictionary<string, byte[]> codes = new Dictionary<string, byte[]>();

        //public byte[] GetScript(byte[] script_hash)
        //{
        //    string strhash = "";
        //    foreach (var b in script_hash)
        //    {
        //        strhash += b.ToString("X02");
        //    }
        //    return codes[strhash];
        //}

        public string GenJson()
        {
            MyJson.JsonNode_Object json = new MyJson.JsonNode_Object();
            json["__name__"] = new MyJson.JsonNode_ValueString("Bhpmodule.");

            //code
            var jsoncode = new MyJson.JsonNode_Array();
            json["code"] = jsoncode;
            foreach (var c in this.total_Codes.Values)
            {
                jsoncode.Add(c.GenJson());
            }
            //code bytes
            var code = this.Build();
            var codestr = "";
            foreach (var c in code)
            {
                codestr += c.ToString("X02");
            }
            json.SetDictValue("codebin", codestr);

            //calls
            MyJson.JsonNode_Object methodinfo = new MyJson.JsonNode_Object();
            json["call"] = methodinfo;
            foreach (var m in this.mapMethods)
            {
                methodinfo[m.Key] = m.Value.GenJson();
            }

            StringBuilder sb = new StringBuilder();
            json.ConvertToStringWithFormat(sb, 4);
            return sb.ToString();
        }
    }

    public class BhpMethod
    {
        public string lastsfieldname = null;//最后一个加载的静态成员的名字，仅event使用

        public int lastparam = -1;//最后一个加载的参数对应
        public int lastCast = -1;

        public bool isEntry = false;
        public string _namespace;
        public string name;
        public string displayName;
        public List<BhpParam> paramtypes = new List<BhpParam>();
        public string returntype;
        public bool isPublic = true;
        public bool inSmartContract;
        public ILMethod method;
        public ILType type;
        //临时变量
        public List<BhpParam> body_Variables = new List<BhpParam>();

        //临时记录在此，会合并到一起
        public SortedDictionary<int, BhpCode> body_Codes = new SortedDictionary<int, BhpCode>();
        public int funcaddr;
        public MyJson.JsonNode_Object GenJson()
        {
            MyJson.JsonNode_Object json = new MyJson.JsonNode_Object();
            json.SetDictValue("name", this.name);
            json.SetDictValue("returntype", this.returntype);
            json.SetDictValue("paramcount", this.paramtypes.Count);
            MyJson.JsonNode_Array jsonparams = new MyJson.JsonNode_Array();
            json.SetDictValue("params", jsonparams);
            for (var i = 0; i < this.paramtypes.Count; i++)
            {
                MyJson.JsonNode_Object item = new MyJson.JsonNode_Object();
                item.SetDictValue("name", this.paramtypes[i].name);
                item.SetDictValue("type", this.paramtypes[i].type);
                jsonparams.Add(item);
            }
            return json;
        }

        //public byte[] Build()
        //{
        //    List<byte> bytes = new List<byte>();
        //    foreach (var c in this.body_Codes.Values)
        //    {
        //        bytes.Add((byte)c.code);
        //        if (c.bytes != null)
        //            for (var i = 0; i < c.bytes.Length; i++)
        //            {
        //                bytes.Add(c.bytes[i]);
        //            }
        //    }
        //    return bytes.ToArray();
        //    //将body链接，生成this.code       byte[]
        //    //并计算 this.codehash            byte[]
        //}

        /// <summary>
        /// Constructor
        /// </summary>
        public BhpMethod() { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="method">Method</param>
        public BhpMethod(ILMethod method)
        {
            this.method = method;
            this.type = method.type;

            foreach (var attr in method.method.CustomAttributes)
            {
                ProcessAttribute(attr);
            }
            _namespace = method.method.DeclaringType.FullName;
            name = method.method.FullName;
            displayName = method.method.Name;
            inSmartContract = method.method.DeclaringType.BaseType.Name == "SmartContract";
            isPublic = method.method.IsPublic;
        }

        private void ProcessAttribute(CustomAttribute attr)
        {
            switch (attr.AttributeType.Name)
            {
                case nameof(DisplayNameAttribute):
                    {
                        displayName = (string)attr.ConstructorArguments[0].Value;
                        break;
                    }
            }
        }
    }

    public class BhpEvent
    {
        public string _namespace;
        public string name;
        public string displayName;
        public List<BhpParam> paramtypes = new List<BhpParam>();

        public BhpEvent(ILField value)
        {
            _namespace = value.field.DeclaringType.FullName;
            name = value.field.DeclaringType.FullName + "::" + value.field.Name;
            displayName = value.displayName;
            paramtypes = value.paramtypes;

            if (value.returntype != "System.Void")
            {
                throw new NotSupportedException($"BRC-3 does not support return types for events. Expected: `System.Void`, Detected: `{value.returntype}`");
            }
        }
    }

    public class BhpCode
    {
        public VM.OpCode code = VM.OpCode.NOP;
        public int addr;
        public byte[] bytes;
        public string debugcode;
        public int debugline = -1;
        public int debugILAddr = -1;
        public string debugILCode;
        public bool needfix = false;//lateparse tag
        public bool needfixfunc = false;
        public int srcaddr;
        public int[] srcaddrswitch;
        public string srcfunc;
        public override string ToString()
        {
            //string info = "AL_" + addr.ToString("X04") + " " + code.ToString();
            //if (bytes != null)
            //    info += " len=" + bytes.Length;
            //if (debugcode != null && debugline >= 0)
            //{
            //    info += "    " + debugcode + "(" + debugline + ")";
            //}

            string info = "" + addr.ToString("X04") + " " + code.ToString();
            for (var j = 0; j < 16 - code.ToString().Length; j++)
            {
                info += " ";
            }
            info += "[";
            if (bytes != null)
            {
                foreach (var c in bytes)
                {
                    info += c.ToString("X02");
                }
            }
            info += "]";

            if (debugcode != null && debugline >= 0)
            {
                info += "//" + debugcode + "(" + debugline + ")";
            }
            return info;
        }

        public MyJson.JsonNode_ValueString GenJson()
        {
            string info = "" + addr.ToString("X04") + " " + code.ToString();
            for (var j = 0; j < 16 - code.ToString().Length; j++)
            {
                info += " ";
            }
            info += "[";
            if (bytes != null)
            {
                foreach (var c in bytes)
                {
                    info += c.ToString("X02");
                }
            }
            info += "]";

            if (debugILCode != null && debugILAddr >= 0)
            {
                info += "<IL_" + debugILAddr.ToString("X04") + " " + debugILCode + ">";
            }
            if (debugcode != null && debugline >= 0)
            {
                info += "//" + debugcode + "(" + debugline + ")";
            }
            return new MyJson.JsonNode_ValueString(info);
        }
    }

    public class BhpField : BhpParam
    {
        public int index { get; private set; }
        public BhpField(string name, string type, int index) : base(name, type)
        {
            this.index = index;
        }
    }

    public class BhpParam
    {
        public string name { get; private set; }
        public string type { get; private set; }
        public BhpParam(string name, string type)
        {
            this.name = name;
            this.type = type;
        }
        public override string ToString()
        {
            return type + " " + name;
        }
    }
}