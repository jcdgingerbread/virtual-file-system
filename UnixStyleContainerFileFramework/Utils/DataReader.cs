using System;
using System.IO;

namespace UnixStyleContainerFileFramework.Utils
{
        internal class DataReader : IDisposable
        {
                private readonly MemoryStream _stream;

                public DataReader(byte[] data)
                {
                        _stream = new MemoryStream(data);
                }

                public void Dispose()
                {
                        _stream.Dispose();
                }

                public uint ReadUInt32()
                {
                        return BitConverter.ToUInt32(Read(sizeof (uint)), 0);
                }

                public byte[] Read(int size)
                {
                        var buffer = new byte[size];

                        _stream.Read(buffer, 0, buffer.Length);

                        return buffer;
                }
        }
}