///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.join.analyze;
using com.espertech.esper.common.@internal.epl.join.querygraph;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.index.advanced.index.service
{
    public class FilterExprAnalyzerAffectorIndexProvision : FilterExprAnalyzerAffector
    {
        private readonly ExprNode[] indexExpressions;
        private readonly IList<Pair<ExprNode, int[]>> keyExpressions;
        private readonly string operationName;
        private readonly int streamNumIndex;

        public FilterExprAnalyzerAffectorIndexProvision(
            string operationName,
            ExprNode[] indexExpressions,
            IList<Pair<ExprNode, int[]>> keyExpressions,
            int streamNumIndex)
        {
            this.operationName = operationName;
            this.indexExpressions = indexExpressions;
            this.keyExpressions = keyExpressions;
            this.streamNumIndex = streamNumIndex;
        }

        public void Apply(QueryGraphForge queryGraph)
        {
            queryGraph.AddCustomIndex(operationName, indexExpressions, keyExpressions, streamNumIndex);
        }
    }
} // end of namespace