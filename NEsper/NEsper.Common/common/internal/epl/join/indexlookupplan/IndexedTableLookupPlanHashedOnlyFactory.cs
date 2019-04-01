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
using com.espertech.esper.common.@internal.epl.@join.exec.@base;
using com.espertech.esper.common.@internal.epl.@join.exec.hash;
using com.espertech.esper.common.@internal.epl.@join.queryplan;

namespace com.espertech.esper.common.@internal.epl.join.indexlookupplan
{
    /// <summary>
    ///     Plan to perform an indexed table lookup.
    /// </summary>
    public class IndexedTableLookupPlanHashedOnlyFactory : TableLookupPlan
    {
        internal readonly EventPropertyValueGetter eventPropertyValueGetter;
        internal readonly ExprEvaluator exprEvaluator;

        public IndexedTableLookupPlanHashedOnlyFactory(
            int lookupStream, int indexedStream, TableLookupIndexReqKey[] indexNum, ExprEvaluator exprEvaluator) : base(
            lookupStream, indexedStream, indexNum)
        {
            this.exprEvaluator = exprEvaluator;
            eventPropertyValueGetter = null;
        }

        public IndexedTableLookupPlanHashedOnlyFactory(
            int lookupStream, int indexedStream, TableLookupIndexReqKey[] indexNum,
            EventPropertyValueGetter eventPropertyValueGetter) : base(lookupStream, indexedStream, indexNum)
        {
            exprEvaluator = null;
            this.eventPropertyValueGetter = eventPropertyValueGetter;
        }

        public ExprEvaluator ExprEvaluator => exprEvaluator;

        public EventPropertyValueGetter EventPropertyValueGetter => eventPropertyValueGetter;

        protected override JoinExecTableLookupStrategy MakeStrategyInternal(
            EventTable[] eventTables, EventType[] eventTypes)
        {
            var index = (PropertyHashedEventTable) eventTables[0];
            if (eventPropertyValueGetter != null) {
                return new IndexedTableLookupStrategyHashedProp(this, index);
            }

            return new IndexedTableLookupStrategyHashedExpr(this, index, eventTypes.Length);
        }
    }
} // end of namespace