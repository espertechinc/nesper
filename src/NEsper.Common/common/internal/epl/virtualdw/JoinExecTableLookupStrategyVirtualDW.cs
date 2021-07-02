///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.hook.vdw;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.join.exec.@base;
using com.espertech.esper.common.@internal.epl.join.querygraph;
using com.espertech.esper.common.@internal.epl.join.queryplan;
using com.espertech.esper.common.@internal.epl.join.rep;
using com.espertech.esper.common.@internal.epl.lookup;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.common.@internal.epl.virtualdw
{
    public class JoinExecTableLookupStrategyVirtualDW : JoinExecTableLookupStrategy
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly ExternalEvaluator[] evaluators;
        private readonly EventBean[] eventsPerStream;
        private readonly VirtualDataWindowLookup externalIndex;
        private readonly int lookupStream;

        private readonly string namedWindowName;

        public JoinExecTableLookupStrategyVirtualDW(
            string namedWindowName,
            VirtualDataWindowLookup externalIndex,
            TableLookupPlan tableLookupPlan)
        {
            this.namedWindowName = namedWindowName;
            this.externalIndex = externalIndex;
            lookupStream = tableLookupPlan.LookupStream;

            var hashKeys = tableLookupPlan.VirtualDWHashEvals;
            if (hashKeys == null) {
                hashKeys = new ExprEvaluator[0];
            }

            var rangeKeys = tableLookupPlan.VirtualDWRangeEvals;
            if (rangeKeys == null) {
                rangeKeys = new QueryGraphValueEntryRange[0];
            }

            evaluators = new ExternalEvaluator[hashKeys.Length + rangeKeys.Length];
            eventsPerStream = new EventBean[lookupStream + 1];

            var count = 0;
            foreach (var hashKey in hashKeys) {
                evaluators[count] = new ExternalEvaluatorHashRelOp(hashKey);
                count++;
            }

            foreach (var rangeKey in rangeKeys) {
                if (rangeKey.Type.IsRange()) {
                    var range = (QueryGraphValueEntryRangeIn) rangeKey;
                    var evaluatorStart = range.ExprStart;
                    var evaluatorEnd = range.ExprEnd;
                    evaluators[count] = new ExternalEvaluatorBtreeRange(evaluatorStart, evaluatorEnd);
                }
                else {
                    var relOp = (QueryGraphValueEntryRangeRelOp) rangeKey;
                    var evaluator = relOp.Expression;
                    evaluators[count] = new ExternalEvaluatorHashRelOp(evaluator);
                }

                count++;
            }
        }

        public LookupStrategyDesc StrategyDesc => new LookupStrategyDesc(LookupStrategyType.VDW);

        public ICollection<EventBean> Lookup(
            EventBean theEvent,
            Cursor cursor,
            ExprEvaluatorContext context)
        {
            eventsPerStream[lookupStream] = theEvent;

            var keys = new object[evaluators.Length];
            for (var i = 0; i < evaluators.Length; i++) {
                keys[i] = evaluators[i].Evaluate(eventsPerStream, context);
            }

            ISet<EventBean> events = null;
            try {
                events = externalIndex.Lookup(keys, eventsPerStream);
            }
            catch (EPException) {
                throw;
            }
            catch (Exception ex) {
                Log.Warn(
                    "Exception encountered invoking virtual data window external index for window '" +
                    namedWindowName +
                    "': " +
                    ex.Message,
                    ex);
            }

            return events;
        }

        public LookupStrategyType LookupStrategyType => LookupStrategyType.VDW;

        public string ToQueryPlan()
        {
            return GetType().GetSimpleName() + " external index " + externalIndex;
        }

        internal interface ExternalEvaluator
        {
            object Evaluate(
                EventBean[] events,
                ExprEvaluatorContext context);
        }

        internal class ExternalEvaluatorHashRelOp : ExternalEvaluator
        {
            private readonly ExprEvaluator hashKeysEval;

            internal ExternalEvaluatorHashRelOp(ExprEvaluator hashKeysEval)
            {
                this.hashKeysEval = hashKeysEval;
            }

            public object Evaluate(
                EventBean[] events,
                ExprEvaluatorContext context)
            {
                return hashKeysEval.Evaluate(events, true, context);
            }
        }

        internal class ExternalEvaluatorBtreeRange : ExternalEvaluator
        {
            private readonly ExprEvaluator endEval;

            private readonly ExprEvaluator startEval;

            internal ExternalEvaluatorBtreeRange(
                ExprEvaluator startEval,
                ExprEvaluator endEval)
            {
                this.startEval = startEval;
                this.endEval = endEval;
            }

            public object Evaluate(
                EventBean[] events,
                ExprEvaluatorContext context)
            {
                var start = startEval.Evaluate(events, true, context);
                var end = endEval.Evaluate(events, true, context);
                return new VirtualDataWindowKeyRange(start, end);
            }
        }
    }
} // end of namespace