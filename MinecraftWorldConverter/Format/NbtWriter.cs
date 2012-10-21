using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using Pdelvo.Minecraft.Network;

namespace MineServer.Map.Format
{
    public class NbtWriter
    {

        private void WriteNode(BigEndianStream zipped, NbtNode node)
        {
            zipped.Write(node.Type);
            if (node.Type == 0) return;
            zipped.Write8(node.Name);
            switch (node.Type)
            {
                case 1:
                    zipped.Write(((NbtByte)node).Value);
                    break;
                case 2:
                    zipped.Write(((NbtShort)node).Value);
                    break;
                case 3:
                    zipped.Write(((NbtInt)node).Value);
                    break;
                case 4:
                    zipped.Write(((NbtLong)node).Value);
                    break;
                case 5:
                    zipped.Write(((NbtFloat)node).Value);
                    break;
                case 6:
                    zipped.Write(((NbtDouble)node).Value);
                    break;
                case 7:
                    zipped.Write(((NbtBytes)node).Value.Length);
                    zipped.Write(((NbtBytes)node).Value);
                    break;
                case 8:
                    zipped.Write8(((NbtString)node).Value);
                    break;
                case 9:
                    var list = (NbtList) node;
                    zipped.Write(list.TagId);
                    zipped.Write(list.Childs.Count);
                    foreach (var child in list.Childs)
                    {
                        WriteRaw(zipped, list.TagId, child);
                    }
                    break;
                case 10:
                    var compount = (NbtCompound) node;
                    foreach (var item in compount.Childs)
                    {
                        WriteNode(zipped, item);
                    }
                        WriteNode(zipped, new NbtStop());
                    break;
                case 11:
                    zipped.Write(((NbtInts)node).Value.Length);
                    zipped.Write(IntToByteArray(((NbtInts)node).Value));
                    break;
            }
        }

        private byte[] IntToByteArray(int[] p)
        {
            byte[] result = new byte[p.Length * 4];
            for (int i = 0; i < p.Length; i++)
            {
                Buffer.BlockCopy(BitConverter.GetBytes(p[i]), 0, result, i*sizeof (int), sizeof (int));
            }

            return result;
        }

        private void WriteRaw(BigEndianStream zipped,byte tagId, object tag)
        {
            switch (tagId)
            {
                case 1:
                    zipped.Write((byte)tag);
                    break;
                case 2:
                    zipped.Write((short)tag);
                    break;
                case 3:
                    zipped.Write((int)tag);
                    break;
                case 4:
                    zipped.Write((long)tag);
                    break;
                case 5:
                    zipped.Write((float)tag);
                    break;
                case 6:
                    zipped.Write((double)tag);
                    break;
              
                case 10:
                    var subNodes = (NbtCompound)tag;
                    foreach (var nbtNode in subNodes.Childs)
                    {
                        WriteNode(zipped, nbtNode);
                    }
                    WriteNode(zipped, new NbtStop());
                    break;
            }
        }

        public void WriteFile(Stream decompressStream, NbtNode node)
        {
            BigEndianStream stream;
            WriteNode(stream = new BigEndianStream(decompressStream), node);
            //stream.Flush();
        }
    }
}
