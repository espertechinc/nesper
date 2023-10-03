using System;
using System.IO;

using Antlr4.Runtime;
using Antlr4.Runtime.Misc;

namespace com.espertech.esper.compiler.@internal.parse
{
    public class CaseChangingCharStream : ICharStream
    {
        private readonly ICharStream stream;
        private readonly bool upper;

        public CaseChangingCharStream(
            ICharStream stream,
            bool upper)
        {
            this.stream = stream;
            this.upper = upper;
        }

        public string GetText(Interval interval)
        {
            return stream.GetText(interval);
        }

        public void Consume()
        {
            stream.Consume();
        }

        public int LA(int i)
        {
            int c = stream.LA(i);
            if (c <= 0) {
                return c;
            }

            if (upper) {
                return Char.ToUpper((char) c);
            }

            return Char.ToLowerInvariant((char) c);
        }

        public int Mark()
        {
            return stream.Mark();
        }

        public void Release(int marker)
        {
            stream.Release(marker);
        }

        public int Index => stream.Index;

        public void Seek(int index)
        {
            stream.Seek(index);
        }

        public int Size => stream.Size;

        public string SourceName => stream.SourceName;
    }
}