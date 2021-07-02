///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.regressionlib.support.filter;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.expr.filter
{
    public class ExprFilterPlanOneFilterNestedFourLvl
    {
        public static IList<FilterTestMultiStmtExecution> Executions(bool withStats)
        {
            IList<FilterTestMultiStmtPermutable> cases = new List<FilterTestMultiStmtPermutable>();

            var stats = FilterTestMultiStmtAssertStats.MakeSingleStat("P0=(fh:1, fi:4),P1=(fh:0, fi:0, fipar:0)");

            // simple four-param equals
            FilterTestMultiStmtPermutable.AddCase(
                cases,
                stats,
                "IntPrimitive=1, LongPrimitive=10, DoublePrimitive=100, BoolPrimitive=true",
                FilterTestMultiStmtAssertItem.MakeItem(SupportBean.MakeBean("A", 1, 10, 100, true), true),
                FilterTestMultiStmtAssertItem.MakeItem(SupportBean.MakeBean("A", 1, 10, 100, false), false),
                FilterTestMultiStmtAssertItem.MakeItem(SupportBean.MakeBean("A", 0, 10, 100, true), false));

            // add boolean-expr
            FilterTestMultiStmtPermutable.AddCase(
                cases,
                stats,
                "IntPrimitive=1, LongPrimitive=10, DoublePrimitive=100, TheString like 'A%'",
                FilterTestMultiStmtAssertItem.MakeItem(SupportBean.MakeBean("A", 1, 10, 100), true),
                FilterTestMultiStmtAssertItem.MakeItem(SupportBean.MakeBean("B", 1, 10, 100), false),
                FilterTestMultiStmtAssertItem.MakeItem(SupportBean.MakeBean("A", 1, 10, 0), false));

            return FilterTestMultiStmtRunner.ComputePermutations(
                typeof(ExprFilterPlanOneFilterNestedFourLvl),
                new PermutationSpec(true),
                cases,
                withStats);
        }
    }
} // end of namespace