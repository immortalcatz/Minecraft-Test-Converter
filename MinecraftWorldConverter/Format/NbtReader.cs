using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using Pdelvo.Minecraft.Network;

namespace MineServer.Map.Format
{
    public class NbtReader
    {

        private NbtNode ReadNode(BigEndianStream zipped)
        {
            byte type = (byte) zipped.ReadByte();
            NbtNode node = null;
            switch (type)
            {
                case 0:
                    return new NbtStop();
                case 1:
                    node = new NbtByte
                    {
                        Name = zipped.ReadString8(),
                        Value = zipped.ReadByte()
                    };
                    break;
                case 2:
                    node = new NbtShort
                    {
                        Name = zipped.ReadString8(),
                        Value = zipped.ReadInt16()
                    };
                    break;
                case 3:
                    node = new NbtInt
                    {
                        Name = zipped.ReadString8(),
                        Value = zipped.ReadInt32()
                    };
                    break;
                case 4:
                    node = new NbtLong
                    {
                        Name = zipped.ReadString8(),
                        Value = zipped.ReadInt64()
                    };
                    break;
                case 5:
                    node = new NbtFloat
                    {
                        Name = zipped.ReadString8(),
                        Value = zipped.ReadSingle()
                    };
                    break;
                case 6:
                    node = new NbtDouble
                    {
                        Name = zipped.ReadString8(),
                        Value = zipped.ReadDouble()
                    };
                    break;
                case 7:
                    node = new NbtBytes
                    {
                        Name = zipped.ReadString8(),
                        Value = zipped.ReadBytes(zipped.ReadInt32())
                    };
                    break;
                case 8:
                    node = new NbtString
                    {
                        Name = zipped.ReadString8(),
                        Value = zipped.ReadString8()
                    };
                    break;
                case 9:
                    node = new NbtList();
                    ((NbtList)node).Name = zipped.ReadString8();
                    ((NbtList)node).Childs = new List<object>();
                    byte tagId = zipped.ReadByte();
                    ((NbtList) node).TagId = tagId;
                    int length = zipped.ReadInt32();

                    for (int i = 0; i < length; i++)
                    {
                        ((NbtList)node).Childs.Add(ReadRaw(zipped, tagId));
                    }
                    break;
                case 10:
                    node = new NbtCompound();
                    ((NbtCompound)node).Name = zipped.ReadString8();
                    ((NbtCompound) node).Childs = new List<NbtNode>();
                    NbtNode subNode;
                    while((subNode = ReadNode(zipped)).Type != 0)
                    {
                        ((NbtCompound) node).Childs.Add(subNode);
                    }
                    break;
                case 11:
                    node = new NbtInts
                    {
                        Name = zipped.ReadString8(),
                        Value = ByteToIntArray(zipped.ReadBytes(zipped.ReadInt32() * sizeof(int)))
                    };
                    break;
            }
            //string str = zipped.ReadString8(100);
            if(node == null) throw new Exception();
            return node;
        }

        private int[] ByteToIntArray(byte[] p)
        {
            int[] result = new int[p.Length/4];
            for (int i = 0; i < p.Length; i+=sizeof(int))
            {
                result[i/4] = BitConverter.ToInt32(p, i);
            }

            return result;
        }

        private object ReadRaw(BigEndianStream zipped, byte tagId)
        {
            switch (tagId)
            {
                case 1:
                    return zipped.ReadByte();
                case 2:
                    return zipped.ReadInt16();
                case 3:
                    return zipped.ReadInt32();
                case 4:
                    return zipped.ReadInt64();
                case 5:
                    return zipped.ReadSingle();
                case 6:
                    return zipped.ReadDouble();
                case 10:
                    var subNodes = new List<NbtNode>();
                    NbtNode subNode;
                    while((subNode = ReadNode(zipped)).Type != 0)
                    {
                        subNodes.Add(subNode);
                    }
                    return subNodes;
            }
            throw new Exception();
        }

        public NbtNode ReadNbtFile(Stream decompressStream)
        {
            return ReadNode(new BigEndianStream(decompressStream));
        }
    }
}
