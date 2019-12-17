///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.index.@base;

namespace com.espertech.esper.common.@internal.collection
{
    public class IterablesArrayEnumerator : IEnumerator<EventBean>
    {
        private readonly IEnumerable<EventBean>[][] _tablesPerRow;
        private IEnumerator<EventBean> _tablesPerRowEnumerator;

        public IterablesArrayEnumerator(EventTable[][] tablesPerRow)
        {
            _tablesPerRow = tablesPerRow;
            _tablesPerRowEnumerator = tablesPerRow
                .Select(tables => tables[0])
                .SelectMany(table => table)
                .GetEnumerator();
        }

        public IterablesArrayEnumerator(IEnumerable<EventBean>[][] tablesPerRow)
        {
            _tablesPerRow = tablesPerRow;
            _tablesPerRowEnumerator = tablesPerRow
                .Select(tables => tables[0])
                .SelectMany(table => table)
                .GetEnumerator();
        }

        public void Dispose()
        {
        }

        public bool MoveNext()
        {
            return _tablesPerRowEnumerator.MoveNext();
        }

        object IEnumerator.Current => _tablesPerRowEnumerator.Current;

        public EventBean Current => _tablesPerRowEnumerator.Current;

        public void Reset()
        {
            _tablesPerRowEnumerator = _tablesPerRow
                .Select(tables => tables[0])
                .SelectMany(table => table)
                .GetEnumerator();
        }
    }
}