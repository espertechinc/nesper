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
    /// MapIndex for filter parameter constants to match using the 'in' operator to match against
    /// a supplied set of values (i.e. multiple possible exact matches). The implementation is 
    /// based on a regular HashMap.
    /// </summary>
    public sealed class FilterParamIndexIn : FilterParamIndexLookupableBase
    {
        private readonly IDictionary<Object, IList<EventEvaluator>> _constantsMap;
        private readonly IDictionary<MultiKeyUntyped, EventEvaluator> _evaluatorsMap;
        private readonly IReaderWriterLock _constantsMapRwLock;

        public FilterParamIndexIn(FilterSpecLookupable lookupable, IReaderWriterLock readWriteLock)
            : base(FilterOperator.IN_LIST_OF_VALUES, lookupable)
        {
            _constantsMap = new Dictionary<Object, IList<EventEvaluator>>().WithNullSupport();
            _evaluatorsMap = new Dictionary<MultiKeyUntyped, EventEvaluator>();
            _constantsMapRwLock = readWriteLock;
        }

        public override EventEvaluator Get(Object filterConstant)
        {
            var keyValues = (MultiKeyUntyped)filterConstant;
            return _evaluatorsMap.Get(keyValues);
        }

        public override void Put(Object filterConstant, EventEvaluator evaluator)
        {
            // Store evaluator keyed to set of values
            var keys = (MultiKeyUntyped)filterConstant;

            // make sure to remove the old evaluator for this constant
            var oldEvaluator = _evaluatorsMap.Push(keys, evaluator);

            // Store each value to match against in Map with it's evaluator as a list
            var keyValues = keys.Keys;
            for (var i = 0; i < keyValues.Length; i++)
            {
                var evaluators = _constantsMap.Get(keyValues[i]);
                if (evaluators == null)
                {
                    evaluators = new List<EventEvaluator>();
                    _constantsMap.Put(keyValues[i], evaluators);
                }
                else
                {
                    if (oldEvaluator != null)
                    {
                        evaluators.Remove(oldEvaluator);
                    }
                }
                evaluators.Add(evaluator);
            }
        }

        public override void Remove(Object filterConstant)
        {
            var keys = (MultiKeyUntyped)filterConstant;

            // remove the mapping of value set to evaluator
            var eval = _evaluatorsMap.Delete(keys);
            var keyValues = keys.Keys;
            for (var i = 0; i < keyValues.Length; i++)
            {
                var evaluators = _constantsMap.Get(keyValues[i]);
                if (evaluators != null) // could be removed already as same-value constants existed
                {
                    evaluators.Remove(eval);
                    if (evaluators.IsEmpty())
                    {
                        _constantsMap.Remove(keyValues[i]);
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

                // Look up in hashtable
                using (_constantsMapRwLock.AcquireReadLock())
                {
                    var evaluators = _constantsMap.Get(attributeValue);

                    // No listener found for the value, return
                    if (evaluators == null)
                    {
                        return;
                    }

                    foreach (var evaluator in evaluators)
                    {
                        evaluator.MatchEvent(theEvent, matches);
                    }

                    returnValue.Value = null;
                }
            }
        }

        private static readonly ILog Log =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
