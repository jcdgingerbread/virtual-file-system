using System;
using System.IO;
using System.Text;

namespace VirtualFileSystem.Utils
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

                private void WriteBytes(byte[] values)
                {
                        _stream.Write(values, 0, values.Length);
                }

                public void WriteUInt32(uint value)
                {
                        _stream.Write(BitConverter.GetBytes(value), 0, sizeof (uint));
                }

                public void WriteByte(byte value)
                {
                        _stream.WriteByte(value);
                }

                private void WriteUInt16(ushort value)
                {
                        _stream.Write(BitConverter.GetBytes(value), 0, sizeof (ushort));
                }

                public void WriteString(string value)
                {
                        WriteUInt16((ushort) value.Length);

                        WriteBytes(Encoding.ASCII.GetBytes(value));
                }

                public byte[] GetData()
                {
                        return _stream.ToArray();
                }
        }
}