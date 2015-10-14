using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace FireDetectiveAnalyzer
{
    public class StringMessageStream
    {
        private Stream m_Stream;
        private Encoding m_ReceiveEncoding;
        private Encoding m_SendEncoding;
        private BinaryReader m_Reader;
        private Func<IEnumerable<byte>, IEnumerable<byte>> m_CorrectEndianness;

        public StringMessageStream(Stream stream, Encoding receiveEncoding, Encoding sendEncoding, bool bigEndian)
        {
            m_Stream = stream;
            m_Reader = new BinaryReader(stream);
            m_ReceiveEncoding = receiveEncoding;
            m_SendEncoding = sendEncoding;
            if (bigEndian)
                m_CorrectEndianness = x => x.Reverse();
            else
                m_CorrectEndianness = x => x;
        }

        public string ReadString()
        {
            byte[] header = m_CorrectEndianness(m_Reader.ReadBytes(4)).ToArray();
            if (header.Length < 4)
                throw new EndOfStreamException();

            int length = BitConverter.ToInt32(header, 0);
            byte[] payload = m_Reader.ReadBytes(length);
            if (payload.Length < length)
                throw new EndOfStreamException();

            return m_ReceiveEncoding.GetString(payload);
        }

        public byte[] ReadBytes(int length)
        {
            byte[] result = m_Reader.ReadBytes(length);
            if (result.Length != length)
                throw new EndOfStreamException();
            return result;
        }

        public void WriteString(string str)
        {
            byte[] header = BitConverter.GetBytes((int)str.Length);
            byte[] payload = m_SendEncoding.GetBytes(str);
            byte[] packet = m_CorrectEndianness(header).Concat(payload).ToArray();
            m_Stream.Write(packet, 0, packet.Length);
        }
    }
}
