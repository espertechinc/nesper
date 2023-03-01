using System.Collections.Generic;

namespace com.espertech.esper.common.client
{
    public struct EPCompilationUnit
    {
        public string Name;
        public byte[] Image;
        public ICollection<string> TypeNames;
    }
}