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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.@join.plan;
using com.espertech.esper.events;

namespace com.espertech.esper.epl.join.exec.composite
{
    using DataMap = IDictionary<object, object>;
    public class CompositeIndexQueryKeyed
        : CompositeIndexQuery
    {
        private readonly ExprEvaluator[] _evaluators;
        private readonly IList<Type> _keyCoercionTypes;
        private readonly int _lookupStream;
        private readonly EventBean[] _events;
        private readonly bool _isNwOnTrigger;
    
        private CompositeIndexQuery _next;
    
        public CompositeIndexQueryKeyed(bool isNWOnTrigger, int lookupStream, int numStreams, IList<QueryGraphValueEntryHashKeyed> hashKeys, IList<Type> keyCoercionTypes)
        {
            _keyCoercionTypes  = keyCoercionTypes;
            _evaluators = new ExprEvaluator[hashKeys.Count];
            _isNwOnTrigger = isNWOnTrigger;
            _lookupStream = lookupStream;
    
            for (int i = 0; i < _evaluators.Length; i++) {
                _evaluators[i] = hashKeys[i].KeyExpr.ExprEvaluator;
            }
            if (lookupStream != -1) {
                _events = new EventBean[lookupStream + 1];
            }
            else {
                _events = new EventBean[numStreams + 1];
            }
        }

        public CompositeIndexQuery Next
        {
            set { _next = value; }
        }

        public ICollection<EventBean> Get(EventBean theEvent, DataMap parent, ExprEvaluatorContext context)
        {
            _events[_lookupStream] = theEvent;
            var mk = EventBeanUtility.GetMultiKey(_events, _evaluators, context, _keyCoercionTypes);
            var innerIndex = (DataMap)parent.Get(mk);
            if (innerIndex == null) {
                return null;
            }
            return _next.Get(theEvent, innerIndex, context);
        }

        public ISet<EventBean> GetCollectKeys(EventBean theEvent, DataMap parent, ExprEvaluatorContext context, IList<Object> keys)
        {
            _events[_lookupStream] = theEvent;
            var mk = EventBeanUtility.GetMultiKey(_events, _evaluators, context, _keyCoercionTypes);
            keys.AddAll(mk.Keys);
            var innerIndex = (DataMap)parent.Get(mk);
            if (innerIndex == null) {
                return null;
            }
            return _next.GetCollectKeys(theEvent, innerIndex, context, keys);
        }

        public ICollection<EventBean> Get(EventBean[] eventsPerStream, DataMap parent, ExprEvaluatorContext context)
        {
    
            EventBean[] eventsToUse;
            if (_isNwOnTrigger) {
                eventsToUse = eventsPerStream;
            }
            else {
                Array.Copy(eventsPerStream, 0, _events, 1, eventsPerStream.Length);
                eventsToUse = _events;
            }
    
            var mk = EventBeanUtility.GetMultiKey(eventsToUse, _evaluators, context, _keyCoercionTypes);
            var innerIndex = (DataMap)parent.Get(mk);
            if (innerIndex == null) {
                return null;
            }
            return _next.Get(eventsPerStream, innerIndex, context);
        }

        public ISet<EventBean> GetCollectKeys(EventBean[] eventsPerStream, DataMap parent, ExprEvaluatorContext context, IList<Object> keys)
        {
            EventBean[] eventsToUse;
            if (_isNwOnTrigger) {
                eventsToUse = eventsPerStream;
            }
            else {
                Array.Copy(eventsPerStream, 0, _events, 1, eventsPerStream.Length);
                eventsToUse = _events;
            }
    
            MultiKeyUntyped mk = EventBeanUtility.GetMultiKey(eventsToUse, _evaluators, context, _keyCoercionTypes);
            keys.AddAll(mk.Keys);
            var innerIndex = (DataMap)parent.Get(mk);
            if (innerIndex == null) {
                return null;
            }
            return _next.GetCollectKeys(eventsPerStream, innerIndex, context, keys);
        }

        public void Add(EventBean theEvent, DataMap value, ISet<EventBean> result)
        {
            throw new UnsupportedOperationException();
        }

        public void Add(EventBean[] eventsPerStream, DataMap value, ISet<EventBean> result)
        {
            throw new UnsupportedOperationException();
        }
    }
}
