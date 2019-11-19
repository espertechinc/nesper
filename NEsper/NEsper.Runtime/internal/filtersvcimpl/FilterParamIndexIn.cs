///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.common.@internal.filtersvc;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compat.threading.locks;
using com.espertech.esper.runtime.@internal.metrics.instrumentation;

namespace com.espertech.esper.runtime.@internal.filtersvcimpl
{
    /// <summary>
    ///     Index for filter parameter constants to match using the 'in' operator to match against a supplied set of values
    ///     (i.e. multiple possible exact matches).
    ///     The implementation is based on a regular HashMap.
    /// </summary>
    public class FilterParamIndexIn : FilterParamIndexLookupableBase
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(FilterParamIndexIn));

        private readonly IDictionary<object, IList<EventEvaluator>> constantsMap;
        private readonly IReaderWriterLock constantsMapRWLock;
        private readonly IDictionary<HashableMultiKey, EventEvaluator> evaluatorsMap;

        public FilterParamIndexIn(
            ExprFilterSpecLookupable lookupable,
            IReaderWriterLock readWriteLock)
            : base(FilterOperator.IN_LIST_OF_VALUES, lookupable)
        {
            constantsMap = new HashMap<object, IList<EventEvaluator>>();
            evaluatorsMap = new HashMap<HashableMultiKey, EventEvaluator>();
            constantsMapRWLock = readWriteLock;
        }

        public override int CountExpensive => constantsMap.Count;

        public override bool IsEmpty => constantsMap.IsEmpty();

        public override IReaderWriterLock ReadWriteLock => constantsMapRWLock;

        public override EventEvaluator Get(object filterConstant)
        {
            var keyValues = (HashableMultiKey) filterConstant;
            return evaluatorsMap.Get(keyValues);
        }

        public override void Put(
            object filterConstant,
            EventEvaluator evaluator)
        {
            // Store evaluator keyed to set of values
            var keys = (HashableMultiKey) filterConstant;

            // make sure to remove the old evaluator for this constant
            EventEvaluator oldEvaluator = evaluatorsMap.Push(keys, evaluator);

            // Store each value to match against in Map with it's evaluator as a list
            var keyValues = keys.Keys;
            for (var i = 0; i < keyValues.Length; i++) {
                var evaluators = constantsMap.Get(keyValues[i]);
                if (evaluators == null) {
                    evaluators = new List<EventEvaluator>();
                    constantsMap.Put(keyValues[i], evaluators);
                }
                else {
                    if (oldEvaluator != null) {
                        evaluators.Remove(oldEvaluator);
                    }
                }

                evaluators.Add(evaluator);
            }
        }

        public override void Remove(object filterConstant)
        {
            var keys = (HashableMultiKey) filterConstant;

            // remove the mapping of value set to evaluator
            EventEvaluator eval = evaluatorsMap.Delete(keys);

            var keyValues = keys.Keys;
            for (var i = 0; i < keyValues.Length; i++) {
                var evaluators = constantsMap.Get(keyValues[i]);
                if (evaluators != null) {
                    // could be removed already as same-value constants existed
                    evaluators.Remove(eval);
                    if (evaluators.IsEmpty()) {
                        constantsMap.Remove(keyValues[i]);
                    }
                }
            }
        }

        public override void MatchEvent(
            EventBean theEvent,
            ICollection<FilterHandle> matches)
        {
            object attributeValue = Lookupable.Getter.Get(theEvent);
            if (InstrumentationHelper.ENABLED) {
                InstrumentationHelper.Get().QFilterReverseIndex(this, attributeValue);
            }

            if (attributeValue == null) {
                if (InstrumentationHelper.ENABLED) {
                    InstrumentationHelper.Get().AFilterReverseIndex(false);
                }

                return;
            }

            // Look up in hashtable
            using (constantsMapRWLock.ReadLock.Acquire()) {
                var evaluators = constantsMap.Get(attributeValue);

                // No listener found for the value, return
                if (evaluators == null) {
                    if (InstrumentationHelper.ENABLED) {
                        InstrumentationHelper.Get().AFilterReverseIndex(false);
                    }

                    return;
                }

                foreach (var evaluator in evaluators) {
                    evaluator.MatchEvent(theEvent, matches);
                }
            }

            if (InstrumentationHelper.ENABLED) {
                InstrumentationHelper.Get().AFilterReverseIndex(null);
            }
        }

        public override void GetTraverseStatement(
            EventTypeIndexTraverse traverse,
            ICollection<int> statementIds,
            ArrayDeque<FilterItem> evaluatorStack)
        {
            foreach (var entry in evaluatorsMap) {
                evaluatorStack.Add(new FilterItem(Lookupable.Expression, FilterOperator, entry.Value));
                entry.Value.GetTraverseStatement(traverse, statementIds, evaluatorStack);
                evaluatorStack.RemoveLast();
            }
        }
    }
} // end of namespace