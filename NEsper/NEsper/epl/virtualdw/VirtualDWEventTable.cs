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
using com.espertech.esper.client.hook;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.@join.table;

namespace com.espertech.esper.epl.virtualdw
{
    public class VirtualDWEventTable : EventTable
    {
        private readonly bool _unique;
        private readonly IList<VirtualDataWindowLookupFieldDesc> _hashAccess;
        private readonly IList<VirtualDataWindowLookupFieldDesc> _btreeAccess;
        private readonly EventTableOrganization _organization;

        public VirtualDWEventTable(
            bool unique,
            IList<VirtualDataWindowLookupFieldDesc> hashAccess,
            IList<VirtualDataWindowLookupFieldDesc> btreeAccess,
            EventTableOrganization organization)
        {
            _unique = unique;
            _hashAccess = hashAccess.AsReadOnlyList();
            _btreeAccess = btreeAccess.AsReadOnlyList();
            _organization = organization;
        }

        public void AddRemove(EventBean[] newData, EventBean[] oldData, ExprEvaluatorContext exprEvaluatorContext)
        {
            Add(newData, exprEvaluatorContext);
            Remove(oldData, exprEvaluatorContext);
        }

        public void Add(EventBean[] events, ExprEvaluatorContext exprEvaluatorContext)
        {
        }

        public void Remove(EventBean[] events, ExprEvaluatorContext exprEvaluatorContext)
        {
        }

        public void Add(EventBean theEvent, ExprEvaluatorContext exprEvaluatorContext)
        {
        }

        public void Remove(EventBean theEvent, ExprEvaluatorContext exprEvaluatorContext)
        {
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<EventBean> GetEnumerator()
        {
            return Collections.GetEmptyList<EventBean>().GetEnumerator();
        }

        public bool IsEmpty()
        {
            return true;
        }

        public void Clear()
        {
        }

        public void Destroy()
        {
        }

        public String ToQueryPlan()
        {
            return "(external event table)";
        }

        public IList<VirtualDataWindowLookupFieldDesc> HashAccess
        {
            get { return _hashAccess; }
        }

        public IList<VirtualDataWindowLookupFieldDesc> BtreeAccess
        {
            get { return _btreeAccess; }
        }

        public bool IsUnique
        {
            get { return _unique; }
        }

        public int? NumberOfEvents
        {
            get { return null; }
        }

        public int NumKeys
        {
            get { return 0; }
        }

        public object Index
        {
            get { return null; }
        }

        public EventTableOrganization Organization
        {
            get { return _organization; }
        }

        public Type ProviderClass
        {
            get { return typeof (VirtualDWEventTable); }
        }
    }
}
