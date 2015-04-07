///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client;

namespace com.espertech.esper.epl.join.table
{
    /// <summary>
    /// Enumerator for use by <seealso cref="com.espertech.esper.epl.join.table.PropertySortedEventTable"/>.
    /// </summary>
    public sealed class PropertySortedEventTableEnumerator
    {
        public static IEnumerator<EventBean> Create(IDictionary<Object, ISet<EventBean>> window)
        {
            return window.SelectMany(entry => entry.Value).GetEnumerator();
        }

        public static IEnumerable<EventBean> CreateEnumerable(IDictionary<Object, ISet<EventBean>> window)
        {
            return window.SelectMany(entry => entry.Value);
        }
    }
}
