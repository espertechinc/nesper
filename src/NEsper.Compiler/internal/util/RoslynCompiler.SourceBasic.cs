using System;
using System.Collections.Generic;
using System.Linq;

namespace com.espertech.esper.compiler.@internal.util
{
    public partial class RoslynCompiler
    {
        public class SourceBasic : Source
        {
            public string Name { get; set; }
            public string Code { get; set; }
            public IEnumerable<Type> References => Enumerable.Empty<Type>();

            public SourceBasic()
            {
            }

            public SourceBasic(
                string sourceName,
                string code)
            {
                Name = sourceName;
                Code = code;
            }
        }
    }
}