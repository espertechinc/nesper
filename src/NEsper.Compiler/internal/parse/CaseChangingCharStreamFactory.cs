using System;

using Antlr4.Runtime;

namespace com.espertech.esper.compiler.@internal.parse
{
    public class CaseChangingCharStreamFactory
    {
        public static ICharStream Make(String text)
        {
            return new CaseChangingCharStream(CharStreams.fromString(text), false);
        }
    }
}