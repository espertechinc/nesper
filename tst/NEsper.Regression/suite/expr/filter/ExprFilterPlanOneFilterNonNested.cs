///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.regressionlib.support.filter;

using NUnit.Framework;

using static com.espertech.esper.common.@internal.support.SupportBean;
using static com.espertech.esper.regressionlib.support.filter.FilterTestMultiStmtAssertItem;
using static com.espertech.esper.regressionlib.support.filter.FilterTestMultiStmtPermutable;

namespace com.espertech.esper.regressionlib.suite.expr.filter
{
    public class ExprFilterPlanOneFilterNonNested
    {
        public static IList<FilterTestMultiStmtExecution> Executions(bool withStats)
        {
            IList<FilterTestMultiStmtPermutable> cases = new List<FilterTestMultiStmtPermutable>();

            var stats = FilterTestMultiStmtAssertStats.MakeSingleStat("P0=(fh:1,fi:1),P1=(fh:0,fi:0,fipar:0)");

            // simple equals
            AddCase(
                cases,
                stats,
                "IntPrimitive = 0",
                MakeItem(MakeBean("E1", 0), true),
                MakeItem(MakeBean("E2", 1), false));

            // simple "not-equals"
            AddCase(cases, stats, "TheString != 'A'", MakeItem(MakeBean("B"), true), MakeItem(MakeBean("A"), false));

            // simple greater
            AddCase(
                cases,
                stats,
                "IntPrimitive >= 0",
                MakeItem(MakeBean("E1", 0), true),
                MakeItem(MakeBean("E1", 1), true),
                MakeItem(MakeBean("E2", -1), false));

            // simple "is"
            AddCase(cases, stats, "TheString is null", MakeItem(MakeBean(null), true), MakeItem(MakeBean("A"), false));

            // simple "is-not"
            AddCase(
                cases,
                stats,
                "TheString is not null",
                MakeItem(MakeBean(null), false),
                MakeItem(MakeBean("A"), true));

            // simple boolean expression
            AddCase(cases, stats, "TheString like 'A%'", MakeItem(MakeBean("A"), true), MakeItem(MakeBean("B"), false));
            AddCase(
                cases,
                stats,
                "getLocalValue(TheString) = 'A'",
                MakeItem(MakeBean("A"), true),
                MakeItem(MakeBean("B"), false));

            // nullable-endpoint range handled as boolean
            AddCase(
                cases,
                stats,
                "TheString between null and 'Z'",
                MakeItem(MakeBean("A"), false),
                MakeItem(MakeBean("B"), false));

            return FilterTestMultiStmtRunner.ComputePermutations(
                typeof(ExprFilterPlanOneFilterNonNested),
                new PermutationSpec(true),
                cases,
                withStats);
        }

        public static string GetLocalValue(string value)
        {
            return value;
        }
    }
} // end of namespace