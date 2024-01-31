///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.compile.multikey;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.epl.resultset.core
{
    public class GroupByRollupInfo
    {
        public GroupByRollupInfo(
            ExprNode[] exprNodes,
            AggregationGroupByRollupDescForge rollupDesc,
            IList<StmtClassForgeableFactory> additionalForgeables,
            MultiKeyClassRef optionalMultiKey)
        {
            ExprNodes = exprNodes;
            RollupDesc = rollupDesc;
            AdditionalForgeables = additionalForgeables;
            OptionalMultiKey = optionalMultiKey;
        }

        public ExprNode[] ExprNodes { get; }

        public AggregationGroupByRollupDescForge RollupDesc { get; }

        public IList<StmtClassForgeableFactory> AdditionalForgeables { get; }

        public MultiKeyClassRef OptionalMultiKey { get; }
    }
} // end of namespace