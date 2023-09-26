///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.index.@base;
using com.espertech.esper.common.@internal.epl.index.hash;
using com.espertech.esper.common.@internal.epl.join.exec.@base;
using com.espertech.esper.common.@internal.epl.join.exec.inkeyword;
using com.espertech.esper.common.@internal.epl.join.queryplan;

namespace com.espertech.esper.common.@internal.epl.join.indexlookupplan
{
    /// <summary>
    /// Plan to perform an indexed table lookup.
    /// </summary>
    public class InKeywordTableLookupPlanSingleIdxFactory : TableLookupPlan
    {
        private ExprEvaluator[] expressions;

        public InKeywordTableLookupPlanSingleIdxFactory(
            int lookupStream,
            int indexedStream,
            TableLookupIndexReqKey[] indexNums,
            ExprEvaluator[] expressions)
            : base(lookupStream, indexedStream, indexNums)
        {
            this.expressions = expressions;
        }

        protected override JoinExecTableLookupStrategy MakeStrategyInternal(
            EventTable[] eventTable,
            EventType[] eventTypes)
        {
            var index = (PropertyHashedEventTable)eventTable[0];
            return new InKeywordSingleTableLookupStrategyExpr(this, index);
        }

        public ExprEvaluator[] Expressions => expressions;
    }
} // end of namespace