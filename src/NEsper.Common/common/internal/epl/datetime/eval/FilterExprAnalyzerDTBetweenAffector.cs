///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.join.analyze;
using com.espertech.esper.common.@internal.epl.join.querygraph;
using com.espertech.esper.common.@internal.epl.lookup;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.datetime.eval
{
    public class FilterExprAnalyzerDTBetweenAffector : FilterExprAnalyzerAffector
    {
        private readonly ExprNode end;
        private readonly bool includeHigh;
        private readonly bool includeLow;
        private readonly ExprNode start;
        private readonly string targetPropertyName;
        private readonly int targetStreamNum;
        private readonly EventType[] typesPerStream;

        public FilterExprAnalyzerDTBetweenAffector(
            EventType[] typesPerStream,
            int targetStreamNum,
            string targetPropertyName,
            ExprNode start,
            ExprNode end,
            bool includeLow,
            bool includeHigh)
        {
            this.typesPerStream = typesPerStream;
            this.targetStreamNum = targetStreamNum;
            this.targetPropertyName = targetPropertyName;
            this.start = start;
            this.end = end;
            this.includeLow = includeLow;
            this.includeHigh = includeHigh;
        }

        public ExprNode[] IndexExpressions => null;

        public IList<Pair<ExprNode, int[]>> KeyExpressions => null;

        public AdvancedIndexConfigContextPartition OptionalIndexSpec => null;

        public string OptionalIndexName => null;

        public void Apply(QueryGraphForge queryGraph)
        {
            var targetExpr = ExprNodeUtilityMake.MakeExprIdentNode(typesPerStream, targetStreamNum, targetPropertyName);
            RangeFilterAnalyzer.Apply(targetExpr, start, end, includeLow, includeHigh, false, queryGraph);
        }
    }
} // end of namespace