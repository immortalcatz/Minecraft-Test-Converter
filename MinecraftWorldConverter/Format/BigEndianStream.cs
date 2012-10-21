using System;
using System.IO;
using System.Threading.Tasks;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.Net;

namespace Pdelvo.Minecraft.Network
{
    public class BigEndianStream : Stream
    {
        public object Owner { get; set; }


        public BigEndianStream(Stream stream)
        {
            Net = stream;
        }

        public Stream Net { get; set; }

        public override bool CanRead
        {
            get { return Net.CanRead; }
        }

        public override bool CanSeek
        {
            get { return Net.CanSeek; }
        }

        public override bool CanWrite
        {
            get { return Net.CanWrite; }
        }

        public override long Length
        {
            get { return Net.Length; }
        }

        public override long Position
        {
            get { return Net.Position; }
            set { Net.Position = value; }
        }

        public bool BufferEnabled { get; set; }
        [DebuggerStepThrough]
        public new byte ReadByte()
        {

            int b = Net.ReadByte();
            if (b >= byte.MinValue && b <= byte.MaxValue)
            {
                return (byte)b;
            }
            throw new EndOfStreamException();
        }
        public byte[] ReadBytes(int count)
        {
            var input = new byte[count];

            Read(input, 0, count);
            return (input);
        }

        public async Task<byte[]> ReadBytesAsync(int count)
        {
            var input = new byte[count];

            await ReadAsync(input, 0, count);
            return (input);
        }

        public short ReadInt16()
        {
            return unchecked((short)((ReadByte() << 8) | ReadByte()));
        }

              public int ReadInt32()
        {
            return unchecked((ReadByte() << 24) | (ReadByte() << 16) | (ReadByte() << 8) | ReadByte());
        }

   
        public long ReadInt64()
        {
            unchecked
            {
                byte[] l = new byte[8];
                if (Read(l, 0, l.Length) != 8)
                    throw new EndOfStreamException();

                long p = 0;
                p |= (long)l[0] << 56;
                p |= (long)l[1] << 48;
                p |= (long)l[2] << 40;
                p |= (long)l[3] << 32;
                p |= (long)l[4] << 24;
                p |= (long)l[5] << 16;
                p |= (long)l[6] << 8;
                p |= (long)l[7];
                return p;
            }

        }

        public async Task<long> ReadInt64Async()
        {
            unchecked
            {
                byte[] l = new byte[8];
                if (await ReadAsync(l, 0, l.Length) != 8)
                    throw new EndOfStreamException();

                long p = 0;
                p |= (long)l[0] << 56;
                p |= (long)l[1] << 48;
                p |= (long)l[2] << 40;
                p |= (long)l[3] << 32;
                p |= (long)l[4] << 24;
                p |= (long)l[5] << 16;
                p |= (long)l[6] << 8;
                p |= (long)l[7];
                return p;
            }

        }

        public unsafe float ReadSingle()
        {
            int i = ReadInt32();
            return *(float*)&i;
        }

        public double ReadDouble()
        {
            var r = new byte[8];
            for (int i = 7; i >= 0; i--)
            {
                r[i] = ReadByte();
            }
            return BitConverter.ToDouble(r, 0);
        }


        public string ReadString16()
        {
            int len = ReadInt16();
            if (len < 0)
            {
                throw new ProtocolViolationException("String length less then zero");
            }
            byte[] b = ReadBytes(len * 2);
            return Encoding.BigEndianUnicode.GetString(b);
        }

        public string ReadString8()
        {
            int len = ReadInt16();
            byte[] b = ReadBytes(len);
            return Encoding.UTF8.GetString(b);
        }

 
        public bool ReadBoolean()
        {
            return ReadByte() == 1;
        }

        public void Write(byte data)
        {
            Net.WriteByte(data);
        }

        public void Write(short data)
        {
            Write(unchecked((byte)(data >> 8)));
            Write(unchecked((byte)data));
        }

        public async Task WriteAsync(short data)
        {
            await WriteAsync(unchecked((byte)(data >> 8)));
            await WriteAsync(unchecked((byte)data));
        }

        public void Write(int data)
        {
            Write(unchecked((byte)(data >> 24)));
            Write(unchecked((byte)(data >> 16)));
            Write(unchecked((byte)(data >> 8)));
            Write(unchecked((byte)data));
        }

        public async Task WriteAsync(int data)
        {
            await WriteAsync(unchecked((byte)(data >> 24)));
            await WriteAsync(unchecked((byte)(data >> 16)));
            await WriteAsync(unchecked((byte)(data >> 8)));
            await WriteAsync(unchecked((byte)data));
        }

        public void Write(long data)
        {
            Write(unchecked((byte)(data >> 56)));
            Write(unchecked((byte)(data >> 48)));
            Write(unchecked((byte)(data >> 40)));
            Write(unchecked((byte)(data >> 32)));
            Write(unchecked((byte)(data >> 24)));
            Write(unchecked((byte)(data >> 16)));
            Write(unchecked((byte)(data >> 8)));
            Write(unchecked((byte)data));
        }

        public async Task WriteAsync(long data)
        {
            await WriteAsync(unchecked((byte)(data >> 56)));
            await WriteAsync(unchecked((byte)(data >> 48)));
            await WriteAsync(unchecked((byte)(data >> 40)));
            await WriteAsync(unchecked((byte)(data >> 32)));
            await WriteAsync(unchecked((byte)(data >> 24)));
            await WriteAsync(unchecked((byte)(data >> 16)));
            await WriteAsync(unchecked((byte)(data >> 8)));
            await WriteAsync(unchecked((byte)data));
        }

        public unsafe void Write(float data)
        {
            Write(*(int*)&data);
        }

        public Task WriteAsync(float data)
        {
            int i;
            unsafe
            {
                i = *(int*)&data;
            }
            return WriteAsync(i);
        }

        public unsafe void Write(double data)
        {
            Write(*(long*)&data);
        }

        public Task WriteAsync(double data)
        {
            long i;
            unsafe
            {
                i = *(long*)&data;
            }
            return WriteAsync(i);
        }

        public void Write(string data)
        {
            byte[] b = Encoding.BigEndianUnicode.GetBytes(data ?? "");
            Write((short)(data ?? "").Length);
            Write(b, 0, b.Length);
        }

        public async Task WriteAsync(string data)
        {
            byte[] b = Encoding.BigEndianUnicode.GetBytes(data ?? "");
            await WriteAsync((short)(data ?? "").Length);
            await WriteAsync(b, 0, b.Length);
        }

        public void Write(byte[] data)
        {
            if (data != null)
                Write(data, 0, data.Length);
        }

        public async Task WriteAsync(byte[] data)
        {
            if (data != null)
                await WriteAsync(data, 0, data.Length);
        }

        public void Write8(string data)
        {
            byte[] b = Encoding.UTF8.GetBytes(data ?? "");
            Write((short)b.Length);
            Write(b, 0, b.Length);
        }

        public async Task Write8Async(string data)
        {
            byte[] b = Encoding.UTF8.GetBytes(data ?? "");
            await WriteAsync((short)b.Length);
            await WriteAsync(b, 0, b.Length);
        }

        public void Write(bool data)
        {
            Write((byte)(data ? 1 : 0));
        }

        public Task WriteAsync(bool data)
        {
            return WriteAsync((byte)(data ? 1 : 0));
        }

        public override void Flush()
        {
            Net.Flush();
        }

        public override Task FlushAsync(CancellationToken token)
        {
            return Net.FlushAsync(token);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int cnt = Net.Read(buffer, offset, count);

            return cnt;
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            int cnt = await Net.ReadAsync(buffer, offset, count, cancellationToken);

            return cnt;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return Net.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            Net.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            Net.Write(buffer, offset, count);
        }

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken token)
        {
            return Net.WriteAsync(buffer, offset, count, token);
        }

        public double ReadDoublePacked()
        {
            return ReadInt32() / 32.0;
        }

        public void WriteDoublePacked(double value)
        {
            Write((int)(value * 32.0));
        }

        public Task WriteDoublePackedAsync(double value)
        {
            return WriteAsync((int)(value * 32.0));
        }

        public override void Close()
        {
            Net.Close();
        }

        public override int ReadTimeout
        {
            get
            {
                return Net.ReadTimeout;
            }
            set
            {
                Net.ReadTimeout = value;
            }
        }

        public override int WriteTimeout
        {
            get
            {
                return Net.WriteTimeout;
            }
            set
            {
                Net.WriteTimeout = value;
            }
        }
    }
}