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
using com.espertech.esper.filter;

namespace com.espertech.esper.supportunit.filter
{
    public class SupportFilterServiceImpl : FilterService
    {
        private readonly List<Pair<FilterValueSet, FilterHandle>> _added = 
            new List<Pair<FilterValueSet, FilterHandle>>();
        private readonly List<FilterHandle> _removed = 
            new List<FilterHandle>();

        public long Evaluate(EventBean theEvent, ICollection<FilterHandle> matches)
        {
            throw new UnsupportedOperationException();
        }

        public long Evaluate(EventBean theEvent, ICollection<FilterHandle> matches, int statementId)
        {
            throw new UnsupportedOperationException();
        }

        public FilterServiceEntry Add(FilterValueSet filterValueSet, FilterHandle callback)
        {
            _added.Add(new Pair<FilterValueSet, FilterHandle>(filterValueSet, callback));
            return null;
        }

        public void Remove(FilterHandle callback, FilterServiceEntry filterServiceEntry)
        {
            _removed.Add(callback);
        }

        public long NumEventsEvaluated
        {
            get { throw new UnsupportedOperationException(); }
        }

        public void ResetStats()
        {
            throw new UnsupportedOperationException();
        }

        public List<Pair<FilterValueSet, FilterHandle>> Added
        {
            get { return _added; }
        }

        public List<FilterHandle> Removed
        {
            get { return _removed; }
        }

        public void Dispose()
        {
        }

        public long FiltersVersion
        {
            get { return long.MinValue; }
        }

        public void RemoveType(EventType type)
        {

        }
    }
}
