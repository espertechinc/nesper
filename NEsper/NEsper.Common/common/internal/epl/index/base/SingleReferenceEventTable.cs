///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.index.@base
{
    public class SingleReferenceEventTable : EventTable,
        EventTableAsSet
    {
        private readonly EventTableOrganization _organization;
        private readonly Atomic<ObjectArrayBackedEventBean> _eventReference;

        public SingleReferenceEventTable(
            EventTableOrganization organization,
            Atomic<ObjectArrayBackedEventBean> eventReference)
        {
            this._organization = organization;
            this._eventReference = eventReference;
        }

        public void AddRemove(
            EventBean[] newData,
            EventBean[] oldData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            throw new UnsupportedOperationException();
        }

        public void Add(
            EventBean[] events,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            throw new UnsupportedOperationException();
        }

        public void Add(
            EventBean @event,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            throw new UnsupportedOperationException();
        }

        public void Remove(
            EventBean[] events,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            throw new UnsupportedOperationException();
        }

        public void Remove(
            EventBean @event,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            throw new UnsupportedOperationException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<EventBean> GetEnumerator()
        {
            return EnumerationHelper.SingletonNullable(_eventReference.Get());
        }

        public bool IsEmpty {
            get => _eventReference.Get() == null;
        }

        public void Clear()
        {
            throw new UnsupportedOperationException();
        }

        public void Destroy()
        {
        }

        public string ToQueryPlan()
        {
            return "single-reference";
        }

        public int? NumberOfEvents {
            get => _eventReference.Get() == null ? 0 : 1;
        }

        public int NumKeys {
            get => 0;
        }

        public object Index {
            get => null;
        }

        public EventTableOrganization Organization {
            get => _organization;
        }

        public ISet<EventBean> AllValues()
        {
            EventBean @event = _eventReference.Get();
            if (@event != null) {
                return Collections.SingletonSet(@event);
            }

            return Collections.GetEmptySet<EventBean>();
        }

        public Type ProviderClass {
            get => typeof(SingleReferenceEventTable);
        }
    }
} // end of namespace