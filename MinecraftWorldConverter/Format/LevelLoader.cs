using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using Pdelvo.Minecraft.Network;
using java.util.zip;

namespace MineServer.Map.Format
{
    public class LevelLoader
    {
        private BigEndianStream _filePointer;

        public LevelLoader(string path)
        {
            if(!File.Exists(path))
            {
                CreateRegionFile(path);
            }
            _filePointer = new BigEndianStream(new FileStream(path, FileMode.Open));
        }

        private void CreateRegionFile(string path)
        {
            using (FileStream stream = File.Create(path))
            {
                stream.Write(new byte[32*32*4], 0, 32*32*4); // Write Empty Header
            }
        }

        public NbtNode Read(int x, int z)
        {
            lock (this)
            {
                MoveToHeaderPosition(x, z);

                var index = _filePointer.ReadInt32();
                if (index == 0) return null; // Not yet generated

                int usedSectors = index & 0xFF;
                int sectorOffset = index >> 8;

                _filePointer.Seek(sectorOffset*4096, SeekOrigin.Begin);

                int chunkLength = _filePointer.ReadInt32();
                if (chunkLength == -1) return null; // Not yet generated
                byte compressionMode = _filePointer.ReadByte();

                MemoryStream decompressStream;
                if (compressionMode == 0)
                {

                }
                if (compressionMode == 1)
                {
                    byte[] buffer = new byte[chunkLength];
                    _filePointer.Read(buffer, 0, buffer.Length);
                    buffer = Decompress(buffer);
                    decompressStream = new MemoryStream(buffer);
                }
                else
                {
                    byte[] buffer = new byte[chunkLength];
                    _filePointer.Read(buffer, 0, buffer.Length);
                    var def = new Inflater();
                    decompressStream = new MemoryStream();
                    def.setInput(buffer);
                    int i = 0;
                    buffer = new byte[1024*1024];
                    try
                    {
                        while (!def.finished() && (i = def.inflate(buffer)) > 0)
                        {
                            decompressStream.Write(buffer, 0, i);

                        }
                    }
                    catch
                    {
                        return null;
                    }
                    decompressStream.Seek(0, SeekOrigin.Begin);
                }
                return new NbtReader().ReadNbtFile(decompressStream);
            }
        }

        private void MoveToHeaderPosition(int x, int z)
        {

            _filePointer.Seek(z*32*sizeof (int) + x*sizeof (int), SeekOrigin.Begin);
        }

        public void Write(int x, int z, NbtNode node)
        {
            lock (this)
            {
                NbtWriter writer = new NbtWriter();
                MemoryStream stream = new MemoryStream();
                writer.WriteFile(stream, node);
                byte[] buffer = Compress(stream.ToArray());
                int usedSectors = ((buffer.Length + 5)%4096) == 0
                                      ? (buffer.Length + 5)/4096
                                      : (buffer.Length + 5)/4096 + 1;
                MoveToHeaderPosition(x, z);
                _filePointer.Write(0);

                int sectorPostion = FindFreeSector(usedSectors);
                Console.WriteLine("Saving chunk {0},{1} at position {2}.", x, z, sectorPostion);
                int sector = (sectorPostion << 8) | usedSectors;

                MoveToHeaderPosition(x, z);

                _filePointer.Write(sector);

                _filePointer.Seek(sectorPostion*4096, SeekOrigin.Begin);
                _filePointer.Write(buffer.Length);
                _filePointer.Write((byte) 1); //GZip Compression
                _filePointer.Write(buffer);
                _filePointer.Flush();
            }
        }

        private int FindFreeSector(int usedSectors)
        {
            var sectors = new List<int>();
            for (int i = 0; i < 32*32; i++)
            {
                _filePointer.Seek(i * sizeof(int), SeekOrigin.Begin);
                int sectorInfo = _filePointer.ReadInt32();
                if(sectorInfo != 0)
                    sectors.Add(sectorInfo);
            }
            if (!sectors.Contains(1 << 8)) sectors.Add(1 << 8);
            sectors.Sort();
            int tail = int.MaxValue;
            for (int i = 0; i < sectors.Count; i++)
            {
                int num = sectors[i];
                int usedS = num & 0xFF;
                int sectorOffset = num >> 8;
                if(sectorOffset - tail >= usedSectors)
                {
                    //Found a piece of memory which is big enough
                    return tail;
                }
                tail = sectorOffset + usedS;
            }

            return tail;
        }

        private static byte[] Decompress(byte[] gzip)
        {
            // Create a GZIP stream with decompression mode.
            // ... Then create a buffer and write into while reading from the GZIP stream.
            using (GZipStream stream = new GZipStream(new MemoryStream(gzip), CompressionMode.Decompress))
            {
                const int size = 4096;
                byte[] buffer = new byte[size];
                using (MemoryStream memory = new MemoryStream())
                {
                    int count = 0;
                    do
                    {
                        count = stream.Read(buffer, 0, size);
                        if (count > 0)
                        {
                            memory.Write(buffer, 0, count);
                        }
                    } while (count > 0);
                    return memory.ToArray();
                }
            }
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
