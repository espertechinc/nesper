///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.events;
using com.espertech.esper.events.arr;

namespace com.espertech.esper.epl.join.table
{
    public class SingleReferenceEventTable : EventTable, EventTableAsSet
    {
        private readonly EventTableOrganization _organization;
        private readonly Atomic<ObjectArrayBackedEventBean> _eventReference;
    
        public SingleReferenceEventTable(EventTableOrganization organization, Atomic<ObjectArrayBackedEventBean> eventReference) {
            this._organization = organization;
            this._eventReference = eventReference;
        }
    
        public void AddRemove(EventBean[] newData, EventBean[] oldData, ExprEvaluatorContext exprEvaluatorContext) {
            throw new UnsupportedOperationException();
        }
    
        public void Add(EventBean[] events, ExprEvaluatorContext exprEvaluatorContext) {
            throw new UnsupportedOperationException();
        }
    
        public void Add(EventBean @event, ExprEvaluatorContext exprEvaluatorContext) {
            throw new UnsupportedOperationException();
        }
    
        public void Remove(EventBean[] events, ExprEvaluatorContext exprEvaluatorContext) {
            throw new UnsupportedOperationException();
        }
    
        public void Remove(EventBean @event, ExprEvaluatorContext exprEvaluatorContext) {
            throw new UnsupportedOperationException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<EventBean> GetEnumerator()
        {
            var eventBean = _eventReference.Get();
            if (eventBean != null)
                yield return eventBean;
        }

        public virtual bool IsEmpty()
        {
            return _eventReference.Get() == null;
        }

        public virtual void Clear()
        {
            throw new UnsupportedOperationException();
        }

        public virtual void Destroy()
        {
        }

        public virtual string ToQueryPlan()
        {
            return "single-reference";
        }

        public virtual int? NumberOfEvents
        {
            get { return _eventReference.Get() == null ? 0 : 1; }
        }

        public virtual int NumKeys
        {
            get { return 0; }
        }

        public virtual object Index
        {
            get { return null; }
        }

        public virtual EventTableOrganization Organization
        {
            get { return _organization; }
        }

        public virtual ISet<EventBean> AllValues
        {
            get
            {
                EventBean @event = _eventReference.Get();
                if (@event != null)
                {
                    return Collections.SingletonSet<EventBean>(@event);
                }
                return Collections.GetEmptySet<EventBean>();
            }
        }

        public virtual Type ProviderClass
        {
            get { return typeof (SingleReferenceEventTable); }
        }
    }
}
