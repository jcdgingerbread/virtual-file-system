using System;
using System.IO;

namespace UnixStyleContainerFileFramework.Utils
{
        internal class DataWriter : IDisposable
        {
                private readonly MemoryStream _stream;

                public DataWriter()
                {
                        _stream = new MemoryStream();
                }

                public void Dispose()
                {
                        _stream.Dispose();
                }

                public void WriteBytes(byte[] values)
                {
                        _stream.Write(values, 0, values.Length);
                }

                public void WriteUInt32(uint value)
                {
                        _stream.Write(BitConverter.GetBytes(value), 0, sizeof (uint));
                }

                public byte[] GetData()
                {
                        return _stream.ToArray();
                }
        }
}