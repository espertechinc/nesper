///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.rowregex
{
    /// <summary>
    /// Partition-by implementation for partition state.
    /// </summary>
    public class RegexPartitionStateRepoGroup : RegexPartitionStateRepo
    {
        /// <summary>EmptyFalse state collection initial threshold. </summary>
        public readonly static int INITIAL_COLLECTION_MIN = 100;

        private readonly RegexPartitionStateRepoGroupMeta _meta;
        private readonly RegexPartitionStateRandomAccessGetter _getter;
        private readonly IDictionary<Object, RegexPartitionStateImpl> _states;
        private readonly RegexPartitionStateRepoScheduleStateImpl _optionalIntervalSchedules;

        private int _currentCollectionSize = INITIAL_COLLECTION_MIN;
        private int _eventSequenceNumber;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="getter">for "prev" function access</param>
        /// <param name="meta">general metadata for grouping</param>
        /// <param name="keepScheduleState">if set to <c>true</c> [keep schedule state].</param>
        /// <param name="terminationStateCompare">The termination state compare.</param>
        public RegexPartitionStateRepoGroup(
            RegexPartitionStateRandomAccessGetter getter,
            RegexPartitionStateRepoGroupMeta meta,
            bool keepScheduleState,
            RegexPartitionTerminationStateComparator terminationStateCompare)
        {
            _getter = getter;
            _meta = meta;
            _states = new NullableDictionary<Object, RegexPartitionStateImpl>();
            _optionalIntervalSchedules = keepScheduleState ? new RegexPartitionStateRepoScheduleStateImpl(terminationStateCompare) : null;
        }

        public int IncrementAndGetEventSequenceNum()
        {
            ++_eventSequenceNumber;
            return _eventSequenceNumber;
        }

        public int EventSequenceNum
        {
            get { return _eventSequenceNumber; }
            set { _eventSequenceNumber = value; }
        }

        public RegexPartitionStateRepoScheduleState ScheduleState
        {
            get { return _optionalIntervalSchedules; }
        }

        public void RemoveState(Object partitionKey)
        {
            _states.Remove(partitionKey);
        }

        public RegexPartitionStateRepo CopyForIterate(bool forOutOfOrderReprocessing)
        {
            var copy = new RegexPartitionStateRepoGroup(_getter, _meta, false, null);
            foreach (var entry in _states)
            {
                copy._states[entry.Key] = new RegexPartitionStateImpl(entry.Value.RandomAccess, entry.Key);
            }
            return copy;
        }

        public int RemoveOld(EventBean[] oldData, bool isEmpty, bool[] found)
        {
            int countRemoved = 0;

            if (isEmpty)
            {
                if (_getter == null)
                {
                    // no "prev" used, clear all state
                    countRemoved = StateCount;
                    _states.Clear();
                }
                else
                {
                    foreach (var entry in _states)
                    {
                        countRemoved += entry.Value.NumStates;
                        entry.Value.CurrentStates = Collections.GetEmptyList<RegexNFAStateEntry>();
                    }
                }

                // clear "prev" state
                if (_getter != null)
                {
                    // we will need to remove event-by-event
                    for (var i = 0; i < oldData.Length; i++)
                    {
                        var partitionState = GetState(oldData[i], true) as RegexPartitionStateImpl;
                        if (partitionState == null)
                        {
                            continue;
                        }
                        partitionState.RemoveEventFromPrev(oldData);
                    }
                }

                return countRemoved;
            }

            // we will need to remove event-by-event
            for (var i = 0; i < oldData.Length; i++)
            {
                var partitionState = GetState(oldData[i], true) as RegexPartitionStateImpl;
                if (partitionState == null)
                {
                    continue;
                }

                if (found[i])
                {
                    countRemoved += partitionState.RemoveEventFromState(oldData[i]);
                    var cleared = partitionState.NumStates == 0;
                    if (cleared)
                    {
                        if (_getter == null)
                        {
                            _states.Remove(partitionState.OptionalKeys);
                        }
                    }
                }

                partitionState.RemoveEventFromPrev(oldData[i]);
            }

            return countRemoved;
        }

        public RegexPartitionState GetState(Object key)
        {
            return _states.Get(key);
        }

        public RegexPartitionState GetState(EventBean theEvent, bool isCollect)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QRegExPartition(_meta.PartitionExpressionNodes); }

            // collect unused states
            if ((isCollect) && (_states.Count >= _currentCollectionSize))
            {
                IList<Object> removeList = new List<Object>();
                foreach (var entry in _states)
                {
                    if ((entry.Value.IsEmptyCurrentState) &&
                        (entry.Value.RandomAccess == null || entry.Value.RandomAccess.IsEmpty))
                    {
                        removeList.Add(entry.Key);
                    }
                }

                foreach (var removeKey in removeList)
                {
                    _states.Remove(removeKey);
                }

                if (removeList.Count < (_currentCollectionSize / 5))
                {
                    _currentCollectionSize *= 2;
                }
            }

            var key = GetKeys(theEvent, _meta);

            var state = _states.Get(key);
            if (state != null)
            {
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().ARegExPartition(true, state); }
                return state;
            }

            state = new RegexPartitionStateImpl(_getter, new List<RegexNFAStateEntry>(), key);
            _states.Put(key, state);

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().ARegExPartition(false, state); }
            return state;
        }

        public void Accept(EventRowRegexNFAViewServiceVisitor visitor)
        {
            visitor.VisitPartitioned((IDictionary<object, RegexPartitionState>)_states);
        }

        public bool IsPartitioned
        {
            get { return true; }
        }

        public IDictionary<object, RegexPartitionStateImpl> States
        {
            get { return _states; }
        }

        public int StateCount
        {
            get { return _states.Sum(entry => entry.Value.NumStates); }
        }

        public static Object GetKeys(EventBean theEvent, RegexPartitionStateRepoGroupMeta meta)
        {
            var eventsPerStream = meta.EventsPerStream;
            eventsPerStream[0] = theEvent;

            var partitionExpressions = meta.PartitionExpressions;
            if (partitionExpressions.Length == 1)
            {
                if (InstrumentationHelper.ENABLED)
                {
                    InstrumentationHelper.Get().QExprValue(meta.PartitionExpressionNodes[0], eventsPerStream);
                    var value = partitionExpressions[0].Evaluate(new EvaluateParams(eventsPerStream, true, meta.ExprEvaluatorContext));
                    InstrumentationHelper.Get().AExprValue(value);
                    return value;
                }
                else
                {
                    return partitionExpressions[0].Evaluate(new EvaluateParams(eventsPerStream, true, meta.ExprEvaluatorContext));
                }
            }

            var keys = new Object[partitionExpressions.Length];
            var count = 0;
            var exprEvaluatorContext = meta.ExprEvaluatorContext;
            foreach (var node in partitionExpressions)
            {
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QExprValue(meta.PartitionExpressionNodes[count], eventsPerStream); }
                keys[count] = node.Evaluate(new EvaluateParams(eventsPerStream, true, exprEvaluatorContext));
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AExprValue(keys[count]); }
                count++;
            }
            return new MultiKeyUntyped(keys);
        }

        public void Dispose()
        {
        }
    }
}
