using MineServer.Map.Format;
using Pdelvo.Minecraft.Network;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftWorldConverter
{
    public class NewLevelWriter
    {
        private BigEndianStream _filePointer;
        private short _sectorSize;
        private int _headerSize;

        public NewLevelWriter(Stream str, short sectorSize = 256)
        {
            CreateRegionFile(str, sectorSize);
            _filePointer = new BigEndianStream(str);

            _sectorSize = _filePointer.ReadInt16();
            _headerSize = 32 * 32 * 16 * 4 + 2;
        }

        private void CreateRegionFile(Stream stream, short sectorSize = 128)
        {
            BigEndianStream endianStream = new BigEndianStream(stream);
            {
                endianStream.Write(sectorSize);
                endianStream.Write(new byte[32 * 32 * 16 * 4], 0, 32 * 32 * 16 * 4); // Write Empty Header
                endianStream.Flush();
                stream.Seek(0, SeekOrigin.Begin);
            }
        }

        private void MoveToHeaderPosition(int x, short y, int z)
        {

            _filePointer.Seek(2 +z*16*32*sizeof (int) + y*32*sizeof (int) + x * sizeof(int), SeekOrigin.Begin);
        }
        public List<int> _sizes = new List<int>();
        public void Write(int x, short y, int z, NbtNode node)
        {
            lock (this)
            {
                NbtWriter writer = new NbtWriter();
                MemoryStream stream = new MemoryStream();
                writer.WriteFile(stream, node);
                byte[] buffer = Compress(stream.ToArray());
                _sizes.Add(buffer.Length);
                int usedSectors = ((buffer.Length + 5)%_sectorSize) == 0
                                      ? (buffer.Length + 5) / _sectorSize
                                      : (buffer.Length + 5) / _sectorSize + 1;
                MoveToHeaderPosition(x, y, z);
                _filePointer.Write(0);

                int sectorPostion = FindFreeSector(usedSectors);
                int sector = (sectorPostion << 9) | usedSectors;

                MoveToHeaderPosition(x,y,  z);

                _filePointer.Write(sector);

                _filePointer.Seek(_headerSize + sectorPostion*_sectorSize, SeekOrigin.Begin);
                _filePointer.Write(buffer.Length);
                _filePointer.Write((byte) 1); //GZip Compression
                _filePointer.Write(buffer);
                _filePointer.Flush();
            }
        }

        private int FindFreeSector(int usedSectors)
        {
            var sectors = new List<int>();
            for (int i = 0; i < 32*16*32; i++)
            {
                _filePointer.Seek(2 + i * sizeof(int), SeekOrigin.Begin);
                int sectorInfo = _filePointer.ReadInt32();
                if(sectorInfo != 0)
                    sectors.Add(sectorInfo);
            }
            if (!sectors.Contains(0)) sectors.Add(0);
            sectors.Sort();
            int tail = int.MaxValue;
            for (int i = 0; i < sectors.Count; i++)
            {
                int num = sectors[i];
                int usedS = num & 0x1FF;
                int sectorOffset = num >> 9;
                if(sectorOffset - tail >= usedSectors)
                {
                    //Found a piece of memory which is big enough
                    return tail;
                }
                tail = sectorOffset + usedS;
            }

            return tail;
        }
        private static byte[] Compress(byte[] gzip)
        {
            // Create a GZIP stream with decompression mode.
            // ... Then create a buffer and write into while reading from the GZIP stream.
            using (MemoryStream memory = new MemoryStream())
            using (GZipStream stream = new GZipStream(memory, CompressionMode.Compress))
            {
                stream.Write(gzip, 0, gzip.Length);
                stream.Flush();
                stream.Close();
                return memory.ToArray();
            }
        }
    }
}
