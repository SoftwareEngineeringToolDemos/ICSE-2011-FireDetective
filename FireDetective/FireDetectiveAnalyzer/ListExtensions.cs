using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FireDetectiveAnalyzer
{
    public static class ListExtensions
    {
        public static IntRange GetRangeUsingBinarySearch<T>(this List<T> list, T start, IComparer<T> startComparer, T end, IComparer<T> endComparer)
        {
            int startIndex = list.BinarySearch(start, startComparer);
            if (startIndex < 0) startIndex = ~startIndex;
            int endIndex = list.BinarySearch(end, endComparer);
            if (endIndex < 0) endIndex = ~endIndex; else endIndex++;
            return new IntRange(startIndex, endIndex);
        }

        public static IEnumerable<int> FindIndices<T>(this List<T> list, Predicate<T> func)
        {
            for (int i = 0; i < list.Count; i++)
                if (func(list[i]))
                    yield return i;
        }
    }

    // [Start..End)
    public struct IntRange
    {
        public int Start;
        public int End;
        public IntRange(int start, int end)
        {
            Start = start;
            End = end;
        }

        public IEnumerable<int> Numbers
        {
            get
            {
                for (int i = Start; i < End; i++)
                    yield return i;
            }
        }

        public IEnumerable<int> NumbersReverse
        {
            get
            {
                for (int i = End - 1; i >= Start; i--)
                    yield return i;
            }
        }
    }

    public class PairRef<P, Q>
    {
        public P First { get; set; }
        public Q Second { get; set; }
        public PairRef(P first, Q second)
        {
            First = first;
            Second = second;
        }
    }

    public static class PairRef
    {
        public static PairRef<P, Q> Make<P, Q>(P first, Q second)
        {
            return new PairRef<P, Q>(first, second);
        }
    }

    public class CustomEventArgs<T> : EventArgs
    {
        public T Data { get; private set; }
        public CustomEventArgs(T data) { Data = data; }
    }
}
