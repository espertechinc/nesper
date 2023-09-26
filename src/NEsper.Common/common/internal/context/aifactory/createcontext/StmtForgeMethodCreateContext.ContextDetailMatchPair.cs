using System.Collections.Generic;

using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.fabric;

namespace com.espertech.esper.common.@internal.context.aifactory.createcontext
{
    public partial class StmtForgeMethodCreateContext
    {
        private class ContextDetailMatchPair
        {
            private readonly ContextSpecCondition condition;
            private readonly MatchEventSpec matches;
            private readonly ISet<string> allTags;
            private readonly IList<StmtClassForgeableFactory> additionalForgeables;
            private readonly FabricCharge fabricCharge;

            public ContextDetailMatchPair(
                ContextSpecCondition condition,
                MatchEventSpec matches,
                ISet<string> allTags,
                IList<StmtClassForgeableFactory> additionalForgeables,
                FabricCharge fabricCharge)
            {
                this.condition = condition;
                this.matches = matches;
                this.allTags = allTags;
                this.additionalForgeables = additionalForgeables;
                this.fabricCharge = fabricCharge;
            }

            public ContextSpecCondition Condition => condition;

            public MatchEventSpec Matches => matches;

            public ISet<string> AllTags => allTags;

            public IList<StmtClassForgeableFactory> AdditionalForgeables => additionalForgeables;

            public FabricCharge FabricCharge => fabricCharge;
        }
    }
}