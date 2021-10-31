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
using com.espertech.esper.common.client.hook.vdw;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.index.@base;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.virtualdw
{
    public class VirtualDWEventTable : EventTable
    {
        public VirtualDWEventTable(
            bool unique,
            IList<VirtualDataWindowLookupFieldDesc> hashAccess,
            IList<VirtualDataWindowLookupFieldDesc> btreeAccess,
            EventTableOrganization organization,
            VirtualDWView virtualDwViewMayNull)
        {
            IsUnique = unique;
            HashAccess = Collections.ReadonlyList(hashAccess);
            BtreeAccess = Collections.ReadonlyList(btreeAccess);
            Organization = organization;
            VirtualDWViewMayNull = virtualDwViewMayNull;
        }

        public IList<VirtualDataWindowLookupFieldDesc> HashAccess { get; }

        public IList<VirtualDataWindowLookupFieldDesc> BtreeAccess { get; }

        public bool IsUnique { get; }

        public void AddRemove(
            EventBean[] newData,
            EventBean[] oldData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            Add(newData, exprEvaluatorContext);
            Remove(oldData, exprEvaluatorContext);
        }

        public void Add(
            EventBean[] events,
            ExprEvaluatorContext exprEvaluatorContext)
        {
        }

        public void Remove(
            EventBean[] events,
            ExprEvaluatorContext exprEvaluatorContext)
        {
        }

        public void Add(
            EventBean @event,
            ExprEvaluatorContext exprEvaluatorContext)
        {
        }

        public void Remove(
            EventBean @event,
            ExprEvaluatorContext exprEvaluatorContext)
        {
        }

        public IEnumerator<EventBean> GetEnumerator()
        {
            return EnumerationHelper.Empty<EventBean>();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return VirtualDWViewMayNull != null
                ? VirtualDWViewMayNull.VirtualDataWindow.GetEnumerator()
                : GetEnumerator();
        }

        public bool IsEmpty => true;

        public void Clear()
        {
        }

        public void Destroy()
        {
        }

        public string ToQueryPlan()
        {
            return "(external event table)";
        }

        public int? NumberOfEvents => null;

        public int NumKeys => 0;

        public object Index => null;

        public EventTableOrganization Organization { get; }
        
        public VirtualDWView VirtualDWViewMayNull { get; }

        public Type ProviderClass => typeof(VirtualDWEventTable);
    }
} // end of namespace