using System.Collections.Generic;

using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.fabric;

namespace com.espertech.esper.common.@internal.context.aifactory.createcontext
{
    public partial class StmtForgeMethodCreateContext
    {
        private class PatternValidatedDesc
        {
            private readonly MatchEventSpec matchEventSpec;
            private readonly ISet<string> allTags;
            private readonly IList<StmtClassForgeableFactory> additionalForgeables;
            private readonly FabricCharge fabricCharge;

            public PatternValidatedDesc(
                MatchEventSpec matchEventSpec,
                ISet<string> allTags,
                IList<StmtClassForgeableFactory> additionalForgeables,
                FabricCharge fabricCharge)
            {
                this.matchEventSpec = matchEventSpec;
                this.allTags = allTags;
                this.additionalForgeables = additionalForgeables;
                this.fabricCharge = fabricCharge;
            }

            public MatchEventSpec MatchEventSpec => matchEventSpec;

            public ISet<string> AllTags => allTags;

            public IList<StmtClassForgeableFactory> AdditionalForgeables => additionalForgeables;

            public FabricCharge FabricCharge => fabricCharge;
        }
    }
}