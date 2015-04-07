///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.rowregex
{
    /// <summary>
    /// Partition-by implementation for partition state.
    /// </summary>
    public class RegexPartitionStateRepoGroup : RegexPartitionStateRepo
    {
        /// <summary>Empty state collection initial threshold. </summary>
        public readonly static int INITIAL_COLLECTION_MIN = 100;
    
        private readonly RegexPartitionStateRepoGroupMeta _meta;
        private readonly RegexPartitionStateRandomAccessGetter _getter;
        private readonly IDictionary<Object, RegexPartitionState> _states;
    
        private int _currentCollectionSize = INITIAL_COLLECTION_MIN;
    
        /// <summary>Ctor. </summary>
        /// <param name="getter">for "prev" function access</param>
        /// <param name="meta">general metadata for grouping</param>
        public RegexPartitionStateRepoGroup(RegexPartitionStateRandomAccessGetter getter,
                                            RegexPartitionStateRepoGroupMeta meta)
        {
            _getter = getter;
            _meta = meta;
            _states = new Dictionary<Object, RegexPartitionState>();
        }
    
        public void RemoveState(Object partitionKey)
        {
            _states.Remove(partitionKey);
        }
    
        public RegexPartitionStateRepo CopyForIterate()
        {
            var copy = new RegexPartitionStateRepoGroup(_getter, _meta);
            foreach (var entry in _states)
            {
                copy._states.Put(entry.Key, new RegexPartitionState(entry.Value.RandomAccess, entry.Key, _meta.HasInterval));
            }
            return copy;
        }
    
        public void RemoveOld(EventBean[] oldData, bool isEmpty, bool[] found)
        {
            if (isEmpty)
            {
                if (_getter == null)
                {
                    // no "prev" used, clear all state
                    _states.Clear();
                }
                else
                {
                    foreach (var entry in _states)
                    {
                        entry.Value.CurrentStates.Clear();
                    }
                }
    
                // clear "prev" state
                if (_getter != null)
                {
                    // we will need to remove event-by-event
                    for (var i = 0; i < oldData.Length; i++)
                    {
                        var partitionState = GetState(oldData[i], true);
                        if (partitionState == null)
                        {
                            continue;
                        }
                        partitionState.RemoveEventFromPrev(oldData);
                    }
                }
    
                return;
            }
    
            // we will need to remove event-by-event
            for (var i = 0; i < oldData.Length; i++)
            {
                var partitionState = GetState(oldData[i], true);
                if (partitionState == null)
                {
                    continue;
                }
    
                if (found[i])
                {
                    var cleared = partitionState.RemoveEventFromState(oldData[i]);
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
                    if ((entry.Value.CurrentStates.IsEmpty()) &&
                        (entry.Value.RandomAccess == null || entry.Value.RandomAccess.IsEmpty()))
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
    
            var key = GetKeys(theEvent);
            
            var state = _states.Get(key);
            if (state != null)
            {
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().ARegExPartition(true, state); }
                return state;
            }
    
            state = new RegexPartitionState(_getter, new List<RegexNFAStateEntry>(), key, _meta.HasInterval);
            _states.Put(key, state);
    
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().ARegExPartition(false, state); }
            return state;
        }
    
        public void Accept(EventRowRegexNFAViewServiceVisitor visitor) {
            visitor.VisitPartitioned(_states);
        }

        public bool IsPartitioned
        {
            get { return true; }
        }

        private Object GetKeys(EventBean theEvent)
        {
            var eventsPerStream = _meta.EventsPerStream;
            eventsPerStream[0] = theEvent;
    
            var partitionExpressions = _meta.PartitionExpressions;
            if (partitionExpressions.Length == 1) {
                if (InstrumentationHelper.ENABLED) {
                    InstrumentationHelper.Get().QExprValue(_meta.PartitionExpressionNodes[0], eventsPerStream);
                    var value = partitionExpressions[0].Evaluate(new EvaluateParams(eventsPerStream, true, _meta.ExprEvaluatorContext));
                    InstrumentationHelper.Get().AExprValue(value);
                    return value;
                }
                else {
                    return partitionExpressions[0].Evaluate(new EvaluateParams(eventsPerStream, true, _meta.ExprEvaluatorContext));
                }
            }
    
            var keys = new Object[partitionExpressions.Length];
            var count = 0;
            var exprEvaluatorContext = _meta.ExprEvaluatorContext;
            foreach (var node in partitionExpressions) {
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QExprValue(_meta.PartitionExpressionNodes[count], eventsPerStream); }
                keys[count] = node.Evaluate(new EvaluateParams(eventsPerStream, true, exprEvaluatorContext));
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AExprValue(keys[count]); }
                count++;
            }
            return new MultiKeyUntyped(keys);
        }
    }
}
