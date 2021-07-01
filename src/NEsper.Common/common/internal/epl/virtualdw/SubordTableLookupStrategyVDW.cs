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
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.join.querygraph;
using com.espertech.esper.common.@internal.epl.lookup;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.common.@internal.epl.virtualdw
{
    public class SubordTableLookupStrategyVDW : SubordTableLookupStrategy
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly ExternalEvaluator[] evaluators;
        private readonly EventBean[] eventsLocal;
        private readonly VirtualDataWindowLookup externalIndex;

        private readonly VirtualDWViewFactory factory;
        private readonly bool nwOnTrigger;

        public SubordTableLookupStrategyVDW(
            VirtualDWViewFactory factory,
            SubordTableLookupStrategyFactoryVDW subordTableFactory,
            VirtualDataWindowLookup externalIndex)
        {
            this.factory = factory;
            this.externalIndex = externalIndex;
            nwOnTrigger = subordTableFactory.IsNwOnTrigger;

            var hashKeys = subordTableFactory.HashEvals;
            var hashCoercionTypes = subordTableFactory.HashCoercionTypes;
            var rangeKeys = subordTableFactory.RangeEvals;
            var rangeCoercionTypes = subordTableFactory.RangeCoercionTypes;

            evaluators = new ExternalEvaluator[hashKeys.Length + rangeKeys.Length];
            eventsLocal = new EventBean[subordTableFactory.NumOuterStreams + 1];

            var count = 0;
            foreach (var hashKey in hashKeys) {
                evaluators[count] = new ExternalEvaluatorHashRelOp(hashKeys[count], hashCoercionTypes[count]);
                count++;
            }

            for (var i = 0; i < rangeKeys.Length; i++) {
                var rangeKey = rangeKeys[i];
                if (rangeKey.Type.IsRange()) {
                    var range = (QueryGraphValueEntryRangeIn) rangeKey;
                    var evaluatorStart = range.ExprStart;
                    var evaluatorEnd = range.ExprEnd;
                    evaluators[count] = new ExternalEvaluatorBtreeRange(
                        evaluatorStart,
                        evaluatorEnd,
                        rangeCoercionTypes[i]);
                }
                else {
                    var relOp = (QueryGraphValueEntryRangeRelOp) rangeKey;
                    var evaluator = relOp.Expression;
                    evaluators[count] = new ExternalEvaluatorHashRelOp(evaluator, rangeCoercionTypes[i]);
                }

                count++;
            }
        }

        public ICollection<EventBean> Lookup(
            EventBean[] eventsPerStream,
            ExprEvaluatorContext context)
        {
            EventBean[] events;
            if (nwOnTrigger) {
                events = eventsPerStream;
            }
            else {
                Array.Copy(eventsPerStream, 0, eventsLocal, 1, eventsPerStream.Length);
                events = eventsLocal;
            }

            var keys = new object[evaluators.Length];
            for (var i = 0; i < evaluators.Length; i++) {
                keys[i] = evaluators[i].Evaluate(events, context);
            }

            ISet<EventBean> data = null;
            try {
                data = externalIndex.Lookup(keys, eventsPerStream);
            }
            catch (EPException) {
                throw;
            }
            catch (Exception ex) {
                Log.Warn(
                    "Exception encountered invoking virtual data window external index for window '" +
                    factory.NamedWindowName +
                    "': " +
                    ex.Message,
                    ex);
            }

            return data;
        }

        public string ToQueryPlan()
        {
            return GetType().GetSimpleName() + " external index " + externalIndex;
        }

        public LookupStrategyDesc StrategyDesc => new LookupStrategyDesc(LookupStrategyType.VDW);

        internal interface ExternalEvaluator
        {
            object Evaluate(
                EventBean[] events,
                ExprEvaluatorContext context);
        }

        internal class ExternalEvaluatorHashRelOp : ExternalEvaluator
        {
            private readonly Type coercionType;

            private readonly ExprEvaluator hashKeysEval;

            internal ExternalEvaluatorHashRelOp(
                ExprEvaluator hashKeysEval,
                Type coercionType)
            {
                this.hashKeysEval = hashKeysEval;
                this.coercionType = coercionType;
            }

            public object Evaluate(
                EventBean[] events,
                ExprEvaluatorContext context)
            {
                return EventBeanUtility.Coerce(hashKeysEval.Evaluate(events, true, context), coercionType);
            }
        }

        internal class ExternalEvaluatorBtreeRange : ExternalEvaluator
        {
            private readonly Type coercionType;
            private readonly ExprEvaluator endEval;

            private readonly ExprEvaluator startEval;

            internal ExternalEvaluatorBtreeRange(
                ExprEvaluator startEval,
                ExprEvaluator endEval,
                Type coercionType)
            {
                this.startEval = startEval;
                this.endEval = endEval;
                this.coercionType = coercionType;
            }

            public object Evaluate(
                EventBean[] events,
                ExprEvaluatorContext context)
            {
                var start = EventBeanUtility.Coerce(startEval.Evaluate(events, true, context), coercionType);
                var end = EventBeanUtility.Coerce(endEval.Evaluate(events, true, context), coercionType);
                return new VirtualDataWindowKeyRange(start, end);
            }
        }
    }
} // end of namespace