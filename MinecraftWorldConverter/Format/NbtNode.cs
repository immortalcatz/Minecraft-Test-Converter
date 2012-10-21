using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MineServer.Map.Format
{
    public abstract class NbtNode
    {
        public abstract byte Type { get; }
        public abstract string Name { get; set; }

        public T GetNodeValue<T>()
        {
            var node = this;
            if (typeof(T) == typeof(string)) return (T)(object)((NbtString)node).Value;
            if (typeof(T) == typeof(byte)) return (T)(object)((NbtByte)node).Value;
            if (typeof(T) == typeof(int)) return (T)(object)((NbtInt)node).Value;
            if (typeof(T) == typeof(byte[])) return (T)(object)((NbtBytes)node).Value;
            if (typeof(T) == typeof(double)) return (T)(object)((NbtDouble)node).Value;
            if (typeof(T) == typeof(float)) return (T)(object)((NbtFloat)node).Value;
            if (typeof(T) == typeof(int[])) return (T)(object)((NbtInts)node).Value;
            if (typeof(T) == typeof(long)) return (T)(object)((NbtLong)node).Value;
            if (typeof(T) == typeof(short)) return (T)(object)((NbtShort)node).Value;
            if (typeof(T) == typeof(NbtList)) return (T)(object)((NbtList)node);
            if (typeof(T) == typeof(NbtCompound)) return (T)(object)((NbtCompound)node);
            throw new Exception("Wrong Typ: " + typeof(T));
        }
    }
    public class NbtStop : NbtNode
    {
        public override byte Type
        {
            get { return 0; }
        }

        public override string Name
        {
            get { return null; }
            set { }
        }
    }
    public class NbtByte : NbtNode
    {
        private string _name;
        public override byte Type
        {
            get { return 1; }
        }

        public override string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public byte Value { get; set; }
    }
    public class NbtShort : NbtNode
    {
        private string _name;
        public override byte Type
        {
            get { return 2; }
        }

        public override string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public short Value { get; set; }
    }
    public class NbtInt : NbtNode
    {
        private string _name;
        public override byte Type
        {
            get { return 3; }
        }

        public override string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public int Value { get; set; }
    }
    public class NbtLong : NbtNode
    {
        private string _name;
        public override byte Type
        {
            get { return 4; }
        }

        public override string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public long Value { get; set; }
    }
    public class NbtFloat : NbtNode
    {
        private string _name;
        public override byte Type
        {
            get { return 5; }
        }

        public override string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public float Value { get; set; }
    }
    public class NbtDouble : NbtNode
    {
        private string _name;
        public override byte Type
        {
            get { return 6; }
        }

        public override string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public double Value { get; set; }
    }
    public class NbtBytes : NbtNode
    {
        private string _name;
        public override byte Type
        {
            get { return 7; }
        }

        public override string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public byte[] Value { get; set; }
    }
    public class NbtString : NbtNode
    {
        private string _name;
        public override byte Type
        {
            get { return 8; }
        }

        public override string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public string Value { get; set; }
    }
    public class NbtList : NbtNode
    {
        private string _name;
        public override byte Type
        {
            get { return 9; }
        }

        public override string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public byte TagId { get; set; }
        public List<object> Childs { get; set; }
    }

    public class NbtCompound : NbtNode
    {
        public NbtCompound()
        {
            Childs = new List<NbtNode>();
        }

        private string _name;
        public override byte Type
        {
            get { return 10; }
        }

        public override string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public List<NbtNode> Childs { get; set; }

        public T GetValue<T>(string name)
        {
            var node = Childs.First(a => a.Name == name);
            if (typeof(T) == typeof(string)) return (T)(object)((NbtString)node).Value;
            if (typeof(T) == typeof(byte)) return (T)(object)((NbtByte)node).Value;
            if (typeof(T) == typeof(int)) return (T)(object)((NbtInt)node).Value;
            if (typeof(T) == typeof(byte[])) return (T)(object)((NbtBytes)node).Value;
            if (typeof(T) == typeof(double)) return (T)(object)((NbtDouble)node).Value;
            if (typeof(T) == typeof(float)) return (T)(object)((NbtFloat)node).Value;
            if (typeof(T) == typeof(int[])) return (T)(object)((NbtInts)node).Value;
            if (typeof(T) == typeof(long)) return (T)(object)((NbtLong)node).Value;
            if (typeof(T) == typeof(short)) return (T)(object)((NbtShort)node).Value;
            if (typeof(T) == typeof(NbtList)) return (T)(object)((NbtList)node);
            if (typeof(T) == typeof(NbtCompound)) return (T)(object)((NbtCompound)node);
            throw new Exception("Wrong Typ: " + typeof(T));
        }

        public void AddNode<T>(string name, T value)
        {
            NbtNode node = null;
            if (typeof(T) == typeof(string)) node = new NbtString { Value = (string)(object)value };
            if (typeof(T) == typeof(byte)) node = new NbtByte { Value = (byte)(object)value };
            if (typeof(T) == typeof(int)) node = new NbtInt { Value = (int)(object)value };
            if (typeof(T) == typeof(byte[])) node = new NbtBytes { Value = (byte[])(object)value };
            if (typeof(T) == typeof(double)) node = new NbtDouble { Value = (double)(object)value };
            if (typeof(T) == typeof(float)) node = new NbtFloat { Value = (float)(object)value };
            if (typeof(T) == typeof(int[])) node = new NbtInts { Value = (int[])(object)value };
            if (typeof(T) == typeof(long)) node = new NbtLong { Value = (long)(object)value };
            if (typeof(T) == typeof(short)) node = new NbtShort { Value = (short)(object)value };
            if (typeof(T) == typeof(NbtList)) node = (NbtNode)(object)value;
            if (typeof(T) == typeof(NbtCompound)) node = (NbtNode)(object)value;
            if(node == null)
            throw new Exception("Wrong Typ: " + typeof(T));
            node.Name = name;
            Childs.Add(node);
        }
    }
    public class NbtInts : NbtNode
    {
        private string _name;
        public override byte Type
        {
            get { return 11; }
        }

        public override string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public int[] Value { get; set; }
    }
}
