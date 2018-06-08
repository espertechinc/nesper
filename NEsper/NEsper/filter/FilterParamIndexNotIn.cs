///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compat.threading;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.filter
{
    /// <summary>
    /// Index for filter parameter constants to match using the 'not in' operator to match 
    /// against a all other values then the supplied set of values.
    /// </summary>
    public sealed class FilterParamIndexNotIn : FilterParamIndexLookupableBase
    {
        private readonly IDictionary<Object, ISet<EventEvaluator>> _constantsMap;
        private readonly IDictionary<MultiKeyUntyped, EventEvaluator> _filterValueEvaluators;
        private readonly ICollection<EventEvaluator> _evaluatorsSet;
        private readonly IReaderWriterLock _constantsMapRwLock;

        public FilterParamIndexNotIn(FilterSpecLookupable lookupable, IReaderWriterLock readWriteLock)
            : base(FilterOperator.NOT_IN_LIST_OF_VALUES, lookupable)
        {
            _constantsMap = new Dictionary<object, ISet<EventEvaluator>>().WithNullSupport();
            _filterValueEvaluators = new Dictionary<MultiKeyUntyped, EventEvaluator>();
            _evaluatorsSet = new HashSet<EventEvaluator>();
            _constantsMapRwLock = readWriteLock;
        }

        public override EventEvaluator Get(Object filterConstant)
        {
            var keyValues = (MultiKeyUntyped)filterConstant;
            return _filterValueEvaluators.Get(keyValues);
        }

        public override void Put(Object filterConstant, EventEvaluator evaluator)
        {
            // Store evaluator keyed to set of values
            var keys = (MultiKeyUntyped)filterConstant;
            _filterValueEvaluators.Put(keys, evaluator);
            _evaluatorsSet.Add(evaluator);

            // Store each value to match against in Map with it's evaluator as a list
            var keyValues = keys.Keys;
            foreach (var keyValue in keyValues)
            {
                var evaluators = _constantsMap.Get(keyValue);
                if (evaluators == null)
                {
                    evaluators = new HashSet<EventEvaluator>();
                    _constantsMap.Put(keyValue, evaluators);
                }
                evaluators.Add(evaluator);
            }
        }

        public override void Remove(Object filterConstant)
        {
            var keys = (MultiKeyUntyped)filterConstant;

            // remove the mapping of value set to evaluator
            var eval = _filterValueEvaluators.Delete(keys);
            _evaluatorsSet.Remove(eval);

            var keyValues = keys.Keys;
            foreach (var keyValue in keyValues)
            {
                var evaluators = _constantsMap.Get(keyValue);
                if (evaluators != null) // could already be removed as constants may be the same
                {
                    evaluators.Remove(eval);
                    if (evaluators.IsEmpty())
                    {
                        _constantsMap.Remove(keyValue);
                    }
                }
            }
        }

        public override int Count
        {
            get { return _constantsMap.Count; }
        }

        public override bool IsEmpty
        {
            get { return _constantsMap.IsEmpty(); }
        }

        public override IReaderWriterLock ReadWriteLock
        {
            get { return _constantsMapRwLock; }
        }

        public override void MatchEvent(EventBean theEvent, ICollection<FilterHandle> matches)
        {
            var attributeValue = Lookupable.Getter.Get(theEvent);
            var returnValue = new Mutable<bool?>(false);

            using (Instrument.With(
                i => i.QFilterReverseIndex(this, attributeValue),
                i => i.AFilterReverseIndex(returnValue.Value)))
            {
                if (attributeValue == null)
                {
                    return;
                }

                // Look up in hashtable the set of not-in evaluators
                using (_constantsMapRwLock.AcquireReadLock())
                {
                    ICollection<EventEvaluator> evalNotMatching = _constantsMap.Get(attributeValue);

                    // if all known evaluators are matching, invoke all
                    if (evalNotMatching == null)
                    {
                        foreach (var eval in _evaluatorsSet)
                        {
                            eval.MatchEvent(theEvent, matches);
                        }

                        returnValue.Value = true;
                        return;
                    }

                    // if none are matching, we are done
                    if (evalNotMatching.Count == _evaluatorsSet.Count)
                    {
                        returnValue.Value = false;
                        return;
                    }

                    // handle partial matches: loop through all evaluators and see which one should not be matching, match all else
                    foreach (var eval in _evaluatorsSet)
                    {
                        if (!(evalNotMatching.Contains(eval)))
                        {
                            eval.MatchEvent(theEvent, matches);
                        }
                    }

                    returnValue.Value = null;
                }
            }
        }

        private static readonly ILog Log =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
