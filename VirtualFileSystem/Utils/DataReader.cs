using System;
using System.IO;
using System.Text;

namespace VirtualFileSystem.Utils
{
        internal class DataReader : IDisposable
        {
                private readonly MemoryStream _stream;

                public DataReader(byte[] data)
                {
                        _stream = new MemoryStream(data);
                }

                public long Position
                {
                        get { return _stream.Position; }
                }

                public long Length
                {
                        get { return _stream.Length; }
                }

                public void Dispose()
                {
                        _stream.Dispose();
                }

                public uint ReadUInt32()
                {
                        return BitConverter.ToUInt32(Read(sizeof (uint)), 0);
                }

                private ushort ReadUInt16()
                {
                        return BitConverter.ToUInt16(Read(sizeof (ushort)), 0);
                }

                public string ReadString()
                {
                        return Encoding.ASCII.GetString(Read(ReadUInt16()));
                }

                public byte ReadByte()
                {
                        return (byte) _stream.ReadByte();
                }

                private byte[] Read(int size)
                {
                        var buffer = new byte[size];

                        _stream.Read(buffer, 0, buffer.Length);

                        return buffer;
                }
        }
}