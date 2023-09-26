using System.Collections.Generic;

namespace com.espertech.esper.common.@internal.epl.variable.core
{
    public partial class VariableReadWritePackageForge
    {
        private class CopyMethodDesc
        {
            public CopyMethodDesc(
                string variableName,
                IList<string> propertiesCopied)
            {
                this.VariableName = variableName;
                this.PropertiesCopied = propertiesCopied;
            }

            public string VariableName { get; }

            public IList<string> PropertiesCopied { get; }
        }
    }
}