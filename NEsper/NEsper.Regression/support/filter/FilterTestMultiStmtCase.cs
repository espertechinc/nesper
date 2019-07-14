///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

namespace com.espertech.esper.regressionlib.support.filter
{
    public class FilterTestMultiStmtCase
    {
        public FilterTestMultiStmtCase(
            string[] filters,
            string stats,
            IList<FilterTestMultiStmtAssertItem> items)
        {
            Filters = filters;
            Stats = stats;
            Items = items;
        }

        public string[] Filters { get; }

        public string Stats { get; }

        public IList<FilterTestMultiStmtAssertItem> Items { get; }
    }
} // end of namespace