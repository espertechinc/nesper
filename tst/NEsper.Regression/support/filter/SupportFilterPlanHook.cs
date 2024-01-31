///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using NUnit.Framework;
using NUnit.Framework.Legacy;
using static com.espertech.esper.common.@internal.filterspec.FilterOperator;

namespace com.espertech.esper.regressionlib.support.filter
{
    public class SupportFilterPlanHook : FilterSpecCompileHook
    {
        private static IList<SupportFilterPlanEntry> _entries;

        static SupportFilterPlanHook()
        {
            Reset();
        }

        public void FilterIndexPlan(
            EventType eventType,
            IList<ExprNode> validatedNodes,
            FilterSpecPlanForge plan)
        {
            _entries.Add(new SupportFilterPlanEntry(eventType, plan, validatedNodes));
        }

        public static void Reset()
        {
            _entries = new List<SupportFilterPlanEntry>();
        }

        public static IList<SupportFilterPlanEntry> Entries => _entries;

        public static void AssertPlanSingle(SupportFilterPlan expected)
        {
            if (_entries.Count != 1) {
                Assert.Fail("Zero or multiple entries");
            }

            AssertPlan(expected, _entries[0].Plan);
        }

        public static void AssertPlanSingleByType(
            string eventTypeName,
            SupportFilterPlan expected)
        {
            SupportFilterPlanEntry found = null;
            foreach (var entry in _entries) {
                if (entry.EventType.Name.Equals(eventTypeName)) {
                    if (found != null) {
                        Assert.Fail("found multiple for type " + eventTypeName);
                    }

                    found = entry;
                }
            }

            if (found == null) {
                Assert.Fail("No entry for type " + eventTypeName);
            }

            AssertPlan(expected, found.Plan);
        }

        public static FilterSpecParamForge AssertPlanSingleForTypeAndReset(string typeName)
        {
            SupportFilterPlanEntry found = null;
            foreach (var entry in _entries) {
                if (entry.EventType.Name != typeName) {
                    continue;
                }

                if (found != null) {
                    Assert.Fail("Found multiple");
                }

                found = entry;
            }

            ClassicAssert.IsNotNull(found);
            Reset();
            return found.GetAssertSingle(typeName);
        }

        public static FilterSpecParamForge AssertPlanSingleTripletAndReset(string typeName)
        {
            var entry = AssertPlanSingleAndReset();
            return entry.GetAssertSingle(typeName);
        }

        public static SupportFilterPlanEntry AssertPlanSingleAndReset()
        {
            ClassicAssert.AreEqual(1, _entries.Count);
            var entry = _entries[0];
            Reset();
            return entry;
        }

        public static void AssertPlan(
            SupportFilterPlan expected,
            FilterSpecPlanForge received)
        {
            ClassicAssert.AreEqual(expected.Paths.Length, received.Paths.Length);
            AssertExpressionOpt(expected.ControlConfirm, received.FilterConfirm);
            AssertExpressionOpt(expected.ControlNegate, received.FilterNegate);

            var pathsReceived = new List<FilterSpecPlanPathForge>(Arrays.AsList(received.Paths));
            for (var i = 0; i < expected.Paths.Length; i++) {
                var pathExpected = expected.Paths[i];

                var path = FindPath(pathExpected, pathsReceived);
                if (path == null) {
                    Assert.Fail("Failed to find path: " + pathExpected);
                }

                pathsReceived.Remove(path);
                AssertPlanPath(pathExpected, path);
            }
        }

        private static FilterSpecPlanPathForge FindPath(
            SupportFilterPlanPath pathExpected,
            IList<FilterSpecPlanPathForge> pathsReceived)
        {
            var tripletsExpected = SortTriplets(pathExpected.Triplets);
            FilterSpecPlanPathForge found = null;

            foreach (var pathReceived in pathsReceived) {
                if (pathExpected.Triplets.Length != pathReceived.Triplets.Length) {
                    continue;
                }

                var tripletsReceived = SortTriplets(pathReceived.Triplets);
                var matches = true;
                for (var i = 0; i < tripletsReceived.Length; i++) {
                    var expected = tripletsExpected[i];
                    var received = tripletsReceived[i];
                    if (!expected.Lookupable.Equals(received.Param.Lookupable.Expression) ||
                        !expected.Op.Equals(received.Param.FilterOperator)) {
                        matches = false;
                        break;
                    }

                    var builder = new StringBuilder();
                    received.Param.ValueExprToString(builder, 0);
                    var value = builder.ToString();
                    if (expected.Op == EQUAL) {
                        var textExpected = FilterSpecParamConstantForge.ValueExprToString(expected.Value);
                        if (!textExpected.Equals(value)) {
                            matches = false;
                            break;
                        }
                    }
                    else if (expected.Op == BOOLEAN_EXPRESSION || expected.Op == REBOOL) {
                        var textExpected = FilterSpecParamExprNodeForge.ValueExprToString(expected.Value);
                        if (!textExpected.Equals(value)) {
                            matches = false;
                            break;
                        }
                    }
                    else {
                        throw new IllegalStateException("Filter op " + expected.Op + " not handled");
                    }
                }

                if (matches) {
                    if (found != null) {
                        throw new IllegalStateException("Multiple matches");
                    }

                    found = pathReceived;
                }
            }

            return found;
        }

        private static void AssertExpressionOpt(
            string expected,
            ExprNode expression)
        {
            if (expected == null) {
                ClassicAssert.IsNull(expression);
            }
            else {
                ClassicAssert.AreEqual(expected, ExprNodeUtilityPrint.ToExpressionStringMinPrecedenceSafe(expression));
            }
        }

        private static void AssertPlanPath(
            SupportFilterPlanPath pathExpected,
            FilterSpecPlanPathForge pathReceived)
        {
            AssertExpressionOpt(pathExpected.ControlNegate, pathReceived.PathNegate);
            ClassicAssert.AreEqual(pathExpected.Triplets.Length, pathReceived.Triplets.Length);

            var tripletsReceived = SortTriplets(pathReceived.Triplets);
            var tripletsExpected = SortTriplets(pathExpected.Triplets);

            for (var i = 0; i < tripletsExpected.Length; i++) {
                AssertPlanPathTriplet(tripletsExpected[i], tripletsReceived[i]);
            }
        }

        private static SupportFilterPlanTriplet[] SortTriplets(SupportFilterPlanTriplet[] triplets)
        {
            var sorted = new SupportFilterPlanTriplet[triplets.Length];
            Array.Copy(triplets, 0, sorted, 0, triplets.Length);
            Array.Sort(sorted, (o1, o2) => {
                var comparedLookupable = o1.Lookupable.CompareTo(o2.Lookupable);
                if (comparedLookupable != 0) {
                    return comparedLookupable;
                }

                var comparison = o1.Op.CompareTo(o2.Op);
                if (comparison != 0) {
                    return comparison;
                }

                throw new IllegalStateException("Comparator does not support value comparison");
            });
            return sorted;
        }

        private static FilterSpecPlanPathTripletForge[] SortTriplets(FilterSpecPlanPathTripletForge[] triplets)
        {
            var sorted = new FilterSpecPlanPathTripletForge[triplets.Length];
            Array.Copy(triplets, 0, sorted, 0, triplets.Length);
            Array.Sort(sorted, (o1, o2) => {
                var comparedLookupable = o1.Param.Lookupable.Expression.CompareTo(o2.Param.Lookupable.Expression);
                if (comparedLookupable != 0) {
                    return comparedLookupable;
                }

                var comparison = o1.Param.FilterOperator.CompareTo(o2.Param.FilterOperator);
                if (comparison != 0) {
                    return comparison;
                }

                throw new IllegalStateException("Comparator does not support value comparison");
            });
            return sorted;
        }

        private static void AssertPlanPathTriplet(
            SupportFilterPlanTriplet tripletExpected,
            FilterSpecPlanPathTripletForge tripletReceived)
        {
            AssertExpressionOpt(tripletExpected.ControlConfirm, tripletReceived.TripletConfirm);
            ClassicAssert.AreEqual(tripletExpected.Lookupable, tripletReceived.Param.Lookupable.Expression);
            ClassicAssert.AreEqual(tripletExpected.Op, tripletReceived.Param.FilterOperator);
            var @out = new StringBuilder();
            tripletReceived.Param.ValueExprToString(@out, 0);
            if (tripletExpected.Op == EQUAL) {
                var expected = FilterSpecParamConstantForge.ValueExprToString(tripletExpected.Value);
                ClassicAssert.AreEqual(expected, @out.ToString());
            }
            else if (tripletExpected.Op == BOOLEAN_EXPRESSION || tripletExpected.Op == REBOOL) {
                var expected = FilterSpecParamExprNodeForge.ValueExprToString(tripletExpected.Value);
                ClassicAssert.AreEqual(expected, @out.ToString());
            }
            else {
                Assert.Fail("operator value to-string not supported yet");
            }
        }

        public static SupportFilterPlanTriplet MakeTripletRebool(
            string lookupable,
            string value)
        {
            return MakeTriplet(lookupable, REBOOL, value);
        }

        public static SupportFilterPlanTriplet MakeTriplet(
            string lookupable,
            FilterOperator op,
            string value)
        {
            return MakeTriplet(lookupable, op, value, null);
        }

        public static SupportFilterPlanTriplet MakeTriplet(
            string lookupable,
            FilterOperator op,
            string value,
            string controlConfirm)
        {
            return new SupportFilterPlanTriplet(lookupable, op, value, controlConfirm);
        }

        public static SupportFilterPlanPath MakePathFromSingle(
            string lookupable,
            FilterOperator op,
            string value)
        {
            var triplet = MakeTriplet(lookupable, op, value);
            return new SupportFilterPlanPath(triplet);
        }

        public static SupportFilterPlanPath[] MakePathsFromSingle(
            string lookupable,
            FilterOperator op,
            string value)
        {
            return new[] {MakePathFromSingle(lookupable, op, value)};
        }

        public static SupportFilterPlanPath[] MakePathsFromEmpty()
        {
            var path = new SupportFilterPlanPath();
            return new[] {path};
        }
    }
} // end of namespace