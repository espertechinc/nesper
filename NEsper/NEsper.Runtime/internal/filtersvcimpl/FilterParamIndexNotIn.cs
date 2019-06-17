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
using com.espertech.esper.compat.threading.locks;
using com.espertech.esper.runtime.@internal.metrics.instrumentation;

namespace com.espertech.esper.runtime.@internal.filtersvcimpl
{
    /// <summary>
    ///     Index for filter parameter constants to match using the 'not in' operator to match against a
    ///     all other values then the supplied set of values.
    /// </summary>
    public class FilterParamIndexNotIn : FilterParamIndexLookupableBase
    {
        private readonly IDictionary<object, ISet<EventEvaluator>> constantsMap;
        private readonly ISet<EventEvaluator> evaluatorsSet;
        private readonly IDictionary<HashableMultiKey, EventEvaluator> filterValueEvaluators;

        public FilterParamIndexNotIn(
            ExprFilterSpecLookupable lookupable,
            IReaderWriterLock readWriteLock)
            : base(FilterOperator.NOT_IN_LIST_OF_VALUES, lookupable)
        {
            constantsMap = new Dictionary<object, ISet<EventEvaluator>>();
            filterValueEvaluators = new Dictionary<HashableMultiKey, EventEvaluator>();
            evaluatorsSet = new HashSet<EventEvaluator>();
            ReadWriteLock = readWriteLock;
        }

        public override int CountExpensive => constantsMap.Count;

        public override bool IsEmpty => constantsMap.IsEmpty();

        public override IReaderWriterLock ReadWriteLock { get; }

        public override EventEvaluator Get(object filterConstant)
        {
            var keyValues = (HashableMultiKey) filterConstant;
            return filterValueEvaluators.Get(keyValues);
        }

        public override void Put(
            object filterConstant,
            EventEvaluator evaluator)
        {
            // Store evaluator keyed to set of values
            var keys = (HashableMultiKey) filterConstant;
            filterValueEvaluators.Put(keys, evaluator);
            evaluatorsSet.Add(evaluator);

            // Store each value to match against in Map with it's evaluator as a list
            foreach (var keyValue in keys.Keys) {
                var evaluators = constantsMap.Get(keyValue);
                if (evaluators == null) {
                    evaluators = new HashSet<EventEvaluator>();
                    constantsMap.Put(keyValue, evaluators);
                }

                evaluators.Add(evaluator);
            }
        }

        public override void Remove(object filterConstant)
        {
            var keys = (HashableMultiKey) filterConstant;

            // remove the mapping of value set to evaluator
            var eval = filterValueEvaluators.Delete(keys);
            evaluatorsSet.Remove(eval);

            foreach (var keyValue in keys.Keys) {
                var evaluators = constantsMap.Get(keyValue);
                if (evaluators != null) {
                    // could already be removed as constants may be the same
                    evaluators.Remove(eval);
                    if (evaluators.IsEmpty()) {
                        constantsMap.Remove(keyValue);
                    }
                }
            }
        }

        public override void MatchEvent(
            EventBean theEvent,
            ICollection<FilterHandle> matches)
        {
            var attributeValue = Lookupable.Getter.Get(theEvent);
            if (InstrumentationHelper.ENABLED) {
                InstrumentationHelper.Get().QFilterReverseIndex(this, attributeValue);
            }

            if (attributeValue == null) {
                if (InstrumentationHelper.ENABLED) {
                    InstrumentationHelper.Get().AFilterReverseIndex(false);
                }

                return;
            }

            // Look up in hashtable the set of not-in evaluators
            using (ReadWriteLock.ReadLock.Acquire()) {
                var evalNotMatching = constantsMap.Get(attributeValue);

                // if all known evaluators are matching, invoke all
                if (evalNotMatching == null) {
                    foreach (var eval in evaluatorsSet) {
                        eval.MatchEvent(theEvent, matches);
                    }

                    if (InstrumentationHelper.ENABLED) {
                        InstrumentationHelper.Get().AFilterReverseIndex(true);
                    }

                    return;
                }

                // if none are matching, we are done
                if (evalNotMatching.Count == evaluatorsSet.Count) {
                    if (InstrumentationHelper.ENABLED) {
                        InstrumentationHelper.Get().AFilterReverseIndex(false);
                    }

                    return;
                }

                // handle partial matches: loop through all evaluators and see which one should not be matching, match all else
                foreach (var eval in evaluatorsSet) {
                    if (!evalNotMatching.Contains(eval)) {
                        eval.MatchEvent(theEvent, matches);
                    }
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
            foreach (var entry in filterValueEvaluators) {
                evaluatorStack.Add(new FilterItem(Lookupable.Expression, FilterOperator, entry.Value));
                entry.Value.GetTraverseStatement(traverse, statementIds, evaluatorStack);
                evaluatorStack.RemoveLast();
            }
        }
    }
} // end of namespace