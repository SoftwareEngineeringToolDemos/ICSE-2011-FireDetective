using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace FireDetectiveAnalyzer
{
    public class MemoryStream<T>
    {
        private List<T> m_Data;
        private int m_Pos = 0;

        public MemoryStream(List<T> data)
        {
            m_Data = data;
        }

        public T Read()
        {
            if (m_Pos == m_Data.Count)
                throw new EndOfStreamException();
            return m_Data[m_Pos++];
        }

        public T Peek()
        {
            if (m_Pos == m_Data.Count)
                throw new EndOfStreamException();
            return m_Data[m_Pos];
        }

        public void Match(Func<T, bool> func)
        {
            if (!TryMatch(func))
                throw new MemoryStreamException("Object unmatched.", m_Pos);
        }

        public bool TryMatch(Func<T, bool> func)
        {
            if (func(Peek()))
            {
                Read();
                return true;
            }
            else
                return false;
        }

        public bool EndOfStream
        {
            get { return m_Pos == m_Data.Count; }
        }

        public int Position { get { return m_Pos; } }
    }

    public class MemoryStreamException : ApplicationException
    {
        public int Position { get; set; }

        public MemoryStreamException(string message, int pos)
            : base(message)
        {
            Position = pos;
        }
    }
}
