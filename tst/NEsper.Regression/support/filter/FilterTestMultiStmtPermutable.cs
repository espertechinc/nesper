///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.compat.collections;

namespace com.espertech.esper.regressionlib.support.filter
{
    public class FilterTestMultiStmtPermutable
    {
        public FilterTestMultiStmtPermutable(
            FilterTestMultiStmtAssertStats[] statsPerPermutation,
            params string[] filters)
        {
            StatsPerPermutation = statsPerPermutation;
            Filters = filters;
        }

        public string[] Filters { get; }

        public IList<FilterTestMultiStmtAssertItem> Items { get; } = new List<FilterTestMultiStmtAssertItem>();

        public FilterTestMultiStmtAssertStats[] StatsPerPermutation { get; }

        public static void AddCase(
            IList<FilterTestMultiStmtPermutable> cases,
            FilterTestMultiStmtAssertStats[] statsPerPermutation,
            string filter,
            params FilterTestMultiStmtAssertItem[] items)
        {
            var theCase = new FilterTestMultiStmtPermutable(statsPerPermutation, filter);
            theCase.Items.AddAll(Arrays.AsList(items));
            cases.Add(theCase);
        }

        public static void AddCase(
            IList<FilterTestMultiStmtPermutable> cases,
            FilterTestMultiStmtAssertStats[] statsPerPermutation,
            string filterOne,
            string filterTwo,
            params FilterTestMultiStmtAssertItem[] items)
        {
            var theCase = new FilterTestMultiStmtPermutable(statsPerPermutation, filterOne, filterTwo);
            theCase.Items.AddAll(Arrays.AsList(items));
            cases.Add(theCase);
        }

        public static void AddCase(
            IList<FilterTestMultiStmtPermutable> cases,
            FilterTestMultiStmtAssertStats[] statsPerPermutation,
            string filterOne,
            string filterTwo,
            string filterThree,
            params FilterTestMultiStmtAssertItem[] items)
        {
            var theCase = new FilterTestMultiStmtPermutable(statsPerPermutation, filterOne, filterTwo, filterThree);
            theCase.Items.AddAll(Arrays.AsList(items));
            cases.Add(theCase);
        }
    }
} // end of namespace