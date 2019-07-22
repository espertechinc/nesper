///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.index.hash;
using com.espertech.esper.common.@internal.epl.join.exec.@base;
using com.espertech.esper.common.@internal.epl.join.indexlookupplan;
using com.espertech.esper.common.@internal.epl.join.rep;
using com.espertech.esper.common.@internal.epl.lookup;
using com.espertech.esper.common.@internal.metrics.instrumentation;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.join.exec.inkeyword
{
    /// <summary>
    /// Lookup on an index using a set of expression results as key values.
    /// </summary>
    public class InKeywordMultiTableLookupStrategyExpr : JoinExecTableLookupStrategy
    {
        private readonly InKeywordTableLookupPlanMultiIdxFactory factory;
        private readonly PropertyHashedEventTable[] indexes;
        private readonly EventBean[] eventsPerStream;

        public InKeywordMultiTableLookupStrategyExpr(
            InKeywordTableLookupPlanMultiIdxFactory factory,
            PropertyHashedEventTable[] indexes)
        {
            this.factory = factory;
            this.indexes = indexes;
            this.eventsPerStream = new EventBean[factory.LookupStream + 1];
        }

        public PropertyHashedEventTable[] Index {
            get => indexes;
        }

        public ICollection<EventBean> Lookup(
            EventBean theEvent,
            Cursor cursor,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            InstrumentationCommon instrumentationCommon = exprEvaluatorContext.InstrumentationProvider;
            instrumentationCommon.QIndexJoinLookup(this, indexes[0]);

            eventsPerStream[factory.LookupStream] = theEvent;
            ISet<EventBean> result = InKeywordTableLookupUtil.MultiIndexLookup(
                factory.KeyExpr,
                eventsPerStream,
                exprEvaluatorContext,
                indexes);

            instrumentationCommon.AIndexJoinLookup(result, null);

            return result;
        }

        public LookupStrategyType LookupStrategyType {
            get => LookupStrategyType.INKEYWORDMULTIIDX;
        }
    }
} // end of namespace