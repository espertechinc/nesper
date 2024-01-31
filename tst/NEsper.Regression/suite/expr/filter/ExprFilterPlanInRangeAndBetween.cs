///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.regressionlib.support.filter;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.expr.filter
{
    [TestFixture]
    public class ExprFilterPlanInRangeAndBetween
    {
        public static IList<FilterTestCaseSingleFieldExecution> Executions()
        {
            IList<FilterTestCaseSingleField> testCases = new List<FilterTestCaseSingleField>();
            FilterTestCaseSingleField.AddCase(
                testCases,
                "(TheString > 'b')",
                "TheString",
                new[] { "a", "b", "c", "d" },
                new[] { false, false, true, true });
            FilterTestCaseSingleField.AddCase(
                testCases,
                "(TheString < 'b')",
                "TheString",
                //new[] {"a", "b", "c", "d"},
                //new[] {true, false, false, false}
                new[] { "c" },
                new[] { false }
            );
            FilterTestCaseSingleField.AddCase(
                testCases,
                "(TheString >= 'b')",
                "TheString",
                new[] { "a", "b", "c", "d" },
                new[] { false, true, true, true });
            FilterTestCaseSingleField.AddCase(
                testCases,
                "(TheString <= 'b')",
                "TheString",
                new[] { "a", "b", "c", "d" },
                new[] { true, true, false, false });
            FilterTestCaseSingleField.AddCase(
                testCases,
                "(TheString in ['b':'d'])",
                "TheString",
                new[] { "a", "b", "c", "d", "e" },
                new[] { false, true, true, true, false });
            FilterTestCaseSingleField.AddCase(
                testCases,
                "(TheString in ('b':'d'])",
                "TheString",
                new[] { "a", "b", "c", "d", "e" },
                new[] { false, false, true, true, false });
            FilterTestCaseSingleField.AddCase(
                testCases,
                "(TheString in ['b':'d'))",
                "TheString",
                new[] { "a", "b", "c", "d", "e" },
                new[] { false, true, true, false, false });
            FilterTestCaseSingleField.AddCase(
                testCases,
                "(TheString in ('b':'d'))",
                "TheString",
                new[] { "a", "b", "c", "d", "e" },
                new[] { false, false, true, false, false });
            FilterTestCaseSingleField.AddCase(
                testCases,
                "(BoolPrimitive in (false))",
                "BoolPrimitive",
                new object[] { true, false },
                new[] { false, true });
            FilterTestCaseSingleField.AddCase(
                testCases,
                "(BoolPrimitive in (false, false, false))",
                "BoolPrimitive",
                new object[] { true, false },
                new[] { false, true });
            FilterTestCaseSingleField.AddCase(
                testCases,
                "(BoolPrimitive in (false, true, false))",
                "BoolPrimitive",
                new object[] { true, false },
                new[] { true, true });
            FilterTestCaseSingleField.AddCase(
                testCases,
                "(IntBoxed in (4, 6, 1))",
                "IntBoxed",
                new object[] { 0, 1, 2, 3, 4, 5, 6 },
                new[] { false, true, false, false, true, false, true });
            FilterTestCaseSingleField.AddCase(
                testCases,
                "(IntBoxed in (3))",
                "IntBoxed",
                new object[] { 0, 1, 2, 3, 4, 5, 6 },
                new[] { false, false, false, true, false, false, false });
            FilterTestCaseSingleField.AddCase(
                testCases,
                "(LongBoxed in (3))",
                "LongBoxed",
                new object[] { 0L, 1L, 2L, 3L, 4L, 5L, 6L },
                new[] { false, false, false, true, false, false, false });
            FilterTestCaseSingleField.AddCase(
                testCases,
                "(IntBoxed between 4 and 6)",
                "IntBoxed",
                new object[] { 0, 1, 2, 3, 4, 5, 6 },
                new[] { false, false, false, false, true, true, true });
            FilterTestCaseSingleField.AddCase(
                testCases,
                "(IntBoxed between 2 and 1)",
                "IntBoxed",
                new object[] { 0, 1, 2, 3, 4, 5, 6 },
                new[] { false, true, true, false, false, false, false });
            FilterTestCaseSingleField.AddCase(
                testCases,
                "(IntBoxed between 4 and -1)",
                "IntBoxed",
                new object[] { 0, 1, 2, 3, 4, 5, 6 },
                new[] { true, true, true, true, true, false, false });
            FilterTestCaseSingleField.AddCase(
                testCases,
                "(IntBoxed in [2:4])",
                "IntBoxed",
                new object[] { 0, 1, 2, 3, 4, 5, 6 },
                new[] { false, false, true, true, true, false, false });
            FilterTestCaseSingleField.AddCase(
                testCases,
                "(IntBoxed in (2:4])",
                "IntBoxed",
                new object[] { 0, 1, 2, 3, 4, 5, 6 },
                new[] { false, false, false, true, true, false, false });
            FilterTestCaseSingleField.AddCase(
                testCases,
                "(IntBoxed in [2:4))",
                "IntBoxed",
                new object[] { 0, 1, 2, 3, 4, 5, 6 },
                new[] { false, false, true, true, false, false, false });
            FilterTestCaseSingleField.AddCase(
                testCases,
                "(IntBoxed in (2:4))",
                "IntBoxed",
                new object[] { 0, 1, 2, 3, 4, 5, 6 },
                new[] { false, false, false, true, false, false, false });
            FilterTestCaseSingleField.AddCase(
                testCases,
                "(IntBoxed not between 4 and 6)",
                "IntBoxed",
                new object[] { 0, 1, 2, 3, 4, 5, 6 },
                new[] { true, true, true, true, false, false, false });
            FilterTestCaseSingleField.AddCase(
                testCases,
                "(IntBoxed not between 2 and 1)",
                "IntBoxed",
                new object[] { 0, 1, 2, 3, 4, 5, 6 },
                new[] { true, false, false, true, true, true, true });
            FilterTestCaseSingleField.AddCase(
                testCases,
                "(IntBoxed not between 4 and -1)",
                "IntBoxed",
                new object[] { 0, 1, 2, 3, 4, 5, 6 },
                new[] { false, false, false, false, false, true, true });
            FilterTestCaseSingleField.AddCase(
                testCases,
                "(IntBoxed not in [2:4])",
                "IntBoxed",
                new object[] { 0, 1, 2, 3, 4, 5, 6 },
                new[] { true, true, false, false, false, true, true });
            FilterTestCaseSingleField.AddCase(
                testCases,
                "(IntBoxed not in (2:4])",
                "IntBoxed",
                new object[] { 0, 1, 2, 3, 4, 5, 6 },
                new[] { true, true, true, false, false, true, true });
            FilterTestCaseSingleField.AddCase(
                testCases,
                "(IntBoxed not in [2:4))",
                "IntBoxed",
                new object[] { 0, 1, 2, 3, 4, 5, 6 },
                new[] { true, true, false, false, true, true, true });
            FilterTestCaseSingleField.AddCase(
                testCases,
                "(IntBoxed not in (2:4))",
                "IntBoxed",
                new object[] { 0, 1, 2, 3, 4, 5, 6 },
                new[] { true, true, true, false, true, true, true });
            FilterTestCaseSingleField.AddCase(
                testCases,
                "(TheString not in ['b':'d'])",
                "TheString",
                new[] { "a", "b", "c", "d", "e" },
                new[] { true, false, false, false, true });
            FilterTestCaseSingleField.AddCase(
                testCases,
                "(TheString not in ('b':'d'])",
                "TheString",
                new[] { "a", "b", "c", "d", "e" },
                new[] { true, true, false, false, true });
            FilterTestCaseSingleField.AddCase(
                testCases,
                "(TheString not in ['b':'d'))",
                "TheString",
                new[] { "a", "b", "c", "d", "e" },
                new[] { true, false, false, true, true });
            FilterTestCaseSingleField.AddCase(
                testCases,
                "(TheString not in ('b':'d'))",
                "TheString",
                new[] { "a", "b", "c", "d", "e" },
                new[] { true, true, false, true, true });
            FilterTestCaseSingleField.AddCase(
                testCases,
                "(TheString not in ('a', 'b'))",
                "TheString",
                new[] { "a", "x", "b", "y" },
                new[] { false, true, false, true });
            FilterTestCaseSingleField.AddCase(
                testCases,
                "(BoolPrimitive not in (false))",
                "BoolPrimitive",
                new object[] { true, false },
                new[] { true, false });
            FilterTestCaseSingleField.AddCase(
                testCases,
                "(BoolPrimitive not in (false, false, false))",
                "BoolPrimitive",
                new object[] { true, false },
                new[] { true, false });
            FilterTestCaseSingleField.AddCase(
                testCases,
                "(BoolPrimitive not in (false, true, false))",
                "BoolPrimitive",
                new object[] { true, false },
                new[] { false, false });
            FilterTestCaseSingleField.AddCase(
                testCases,
                "(IntBoxed not in (4, 6, 1))",
                "IntBoxed",
                new object[] { 0, 1, 2, 3, 4, 5, 6 },
                new[] { true, false, true, true, false, true, false });
            FilterTestCaseSingleField.AddCase(
                testCases,
                "(IntBoxed not in (3))",
                "IntBoxed",
                new object[] { 0, 1, 2, 3, 4, 5, 6 },
                new[] { true, true, true, false, true, true, true });
            FilterTestCaseSingleField.AddCase(
                testCases,
                "(LongBoxed not in (3))",
                "LongBoxed",
                new object[] { 0L, 1L, 2L, 3L, 4L, 5L, 6L },
                new[] { true, true, true, false, true, true, true });

            IList<FilterTestCaseSingleFieldExecution> executions = new List<FilterTestCaseSingleFieldExecution>();
            foreach (var testCase in testCases) {
                executions.Add(
                    new FilterTestCaseSingleFieldExecution(
                        typeof(ExprFilterPlanInRangeAndBetween),
                        testCase,
                        "P0-P1=(fh:1,fi:1),P2=(fh:0,fi:0,fipar:0)"));
            }

            return executions;
        }
    }
} // end of namespace