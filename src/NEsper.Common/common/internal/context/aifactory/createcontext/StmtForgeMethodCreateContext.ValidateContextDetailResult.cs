using System.Collections.Generic;

using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.fabric;

namespace com.espertech.esper.common.@internal.context.aifactory.createcontext
{
    public partial class StmtForgeMethodCreateContext
    {
        private class ValidateContextDetailResult
        {
            private readonly IList<StmtClassForgeableFactory> additionalForgables;
            private readonly FabricCharge fabricCharge;

            public IList<StmtClassForgeableFactory> AdditionalForgables => additionalForgables;

            public FabricCharge FabricCharge => fabricCharge;

            public ValidateContextDetailResult(
                IList<StmtClassForgeableFactory> additionalForgables,
                FabricCharge fabricCharge)
            {
                this.additionalForgables = additionalForgables;
                this.fabricCharge = fabricCharge;
            }
        }
    }
}