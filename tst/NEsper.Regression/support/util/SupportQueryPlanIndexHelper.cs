///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Text;

using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.@join.indexlookupplan;
using com.espertech.esper.common.@internal.epl.@join.querygraph;
using com.espertech.esper.common.@internal.epl.@join.queryplan;
using com.espertech.esper.common.@internal.epl.@join.queryplanouter;
using com.espertech.esper.compat.collections;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.regressionlib.support.util
{
    public class SupportQueryPlanIndexHelper
    {
        public static string GetIndexedExpressions(IDictionary<TableLookupIndexReqKey, QueryPlanIndexItemForge> entries)
        {
            var buf = new StringBuilder();
            foreach (var entry in entries) {
                buf.Append(entry.Value.HashProps.RenderAny());
            }

            return buf.ToString();
        }

        public static void CompareQueryPlans(
            QueryPlanForge expectedPlan,
            QueryPlanForge actualPlan)
        {
            IDictionary<TableLookupIndexReqKey, TableLookupIndexReqKey> indexNameMapping =
                new Dictionary<TableLookupIndexReqKey, TableLookupIndexReqKey>();
            CompareIndexes(expectedPlan.IndexSpecs, actualPlan.IndexSpecs, indexNameMapping);
            CompareExecNodeSpecs(expectedPlan.ExecNodeSpecs, actualPlan.ExecNodeSpecs, indexNameMapping);
        }

        private static void CompareIndexes(
            QueryPlanIndexForge[] expected,
            QueryPlanIndexForge[] actual,
            IDictionary<TableLookupIndexReqKey, TableLookupIndexReqKey> indexNameMapping)
        {
            ClassicAssert.AreEqual(expected.Length, actual.Length);
            for (var i = 0; i < expected.Length; i++) {
                CompareIndex(i, expected[i], actual[i], indexNameMapping);
            }
        }

        private static void CompareIndex(
            int streamNum,
            QueryPlanIndexForge expected,
            QueryPlanIndexForge actual,
            IDictionary<TableLookupIndexReqKey, TableLookupIndexReqKey> indexNameMapping)
        {
            var actualItems = actual.Items;
            var expectedItems = expected.Items;
            ClassicAssert.AreEqual(
                expectedItems.Count,
                actualItems.Count,
                "Number of indexes mismatch for stream " + streamNum
            );

            var actualEnum = actualItems.GetEnumerator();
            var expectedEnum = expectedItems.GetEnumerator();

            var count = 0;

            while (true) {
                var actualNext = actualEnum.MoveNext();
                var expectedNext = expectedEnum.MoveNext();
                Assert.That(actualNext, Is.EqualTo(expectedNext));

                if (!actualNext) {
                    break;
                }

                var actualItem = actualEnum.Current;
                var expectedItem = expectedEnum.Current;
                CompareIndexItem(streamNum, count, expectedItem.Value, actualItem.Value);
                count++;
                indexNameMapping.Put(actualItem.Key, expectedItem.Key);
            }
        }

        private static void CompareExecNodeSpecs(
            QueryPlanNodeForge[] expected,
            QueryPlanNodeForge[] actual,
            IDictionary<TableLookupIndexReqKey, TableLookupIndexReqKey> indexNameMapping)
        {
            ClassicAssert.AreEqual(expected.Length, actual.Length);
            for (var i = 0; i < expected.Length; i++) {
                CompareExecNodeSpec(i, expected[i], actual[i], indexNameMapping);
            }
        }

        private static void CompareExecNodeSpec(
            int streamNum,
            QueryPlanNodeForge expected,
            QueryPlanNodeForge actual,
            IDictionary<TableLookupIndexReqKey, TableLookupIndexReqKey> indexNameMapping)
        {
            if (actual is QueryPlanNodeNoOpForge && expected == null) {
            }
            else if (actual is TableLookupNodeForge && expected is TableLookupNodeForge) {
                CompareTableLookup(
                    streamNum,
                    (TableLookupNodeForge) expected,
                    (TableLookupNodeForge) actual,
                    indexNameMapping);
            }
            else if (actual is TableOuterLookupNodeForge && expected is TableOuterLookupNodeForge) {
                CompareTableLookupOuter(
                    streamNum,
                    (TableOuterLookupNodeForge) expected,
                    (TableOuterLookupNodeForge) actual,
                    indexNameMapping);
            }
            else if (actual is LookupInstructionQueryPlanNodeForge && expected is LookupInstructionQueryPlanNodeForge) {
                CompareInstruction(
                    streamNum,
                    (LookupInstructionQueryPlanNodeForge) expected,
                    (LookupInstructionQueryPlanNodeForge) actual,
                    indexNameMapping);
            }
            else {
                Assert.Fail(
                    "Failed to compare plan node for stream " +
                    streamNum +
                    ", unhandled plan " +
                    actual.GetType().Name);
            }
        }

        private static void CompareInstruction(
            int streamNum,
            LookupInstructionQueryPlanNodeForge expected,
            LookupInstructionQueryPlanNodeForge actual,
            IDictionary<TableLookupIndexReqKey, TableLookupIndexReqKey> indexNameMapping)
        {
            ClassicAssert.AreEqual(expected.RootStream, actual.RootStream);
            ClassicAssert.AreEqual(expected.RootStreamName, actual.RootStreamName);
            ClassicAssert.AreEqual(expected.LookupInstructions.Count, actual.LookupInstructions.Count);
            for (var i = 0; i < expected.LookupInstructions.Count; i++) {
                CompareInstructionDetail(
                    streamNum,
                    i,
                    expected.LookupInstructions[i],
                    actual.LookupInstructions[i],
                    indexNameMapping);
            }
        }

        private static void CompareInstructionDetail(
            int streamNum,
            int numInstruction,
            LookupInstructionPlanForge expected,
            LookupInstructionPlanForge actual,
            IDictionary<TableLookupIndexReqKey, TableLookupIndexReqKey> indexNameMapping)
        {
            ClassicAssert.AreEqual(expected.LookupPlans.Length, actual.LookupPlans.Length);
            for (var i = 0; i < expected.LookupPlans.Length; i++) {
                CompareTableLookupPlan(
                    streamNum,
                    numInstruction,
                    expected.LookupPlans[i],
                    actual.LookupPlans[i],
                    indexNameMapping);
            }
        }

        private static void CompareTableLookupOuter(
            int streamNum,
            TableOuterLookupNodeForge expected,
            TableOuterLookupNodeForge actual,
            IDictionary<TableLookupIndexReqKey, TableLookupIndexReqKey> indexNameMapping)
        {
            CompareTableLookupPlan(
                streamNum,
                0,
                expected.LookupStrategySpec,
                actual.LookupStrategySpec,
                indexNameMapping);
        }

        private static void CompareTableLookup(
            int streamNum,
            TableLookupNodeForge expected,
            TableLookupNodeForge actual,
            IDictionary<TableLookupIndexReqKey, TableLookupIndexReqKey> indexNameMapping)
        {
            CompareTableLookupPlan(streamNum, 0, expected.TableLookupPlan, actual.TableLookupPlan, indexNameMapping);
        }

        private static void CompareTableLookupPlan(
            int streamNum,
            int numInstruction,
            TableLookupPlanForge expectedPlan,
            TableLookupPlanForge actualPlan,
            IDictionary<TableLookupIndexReqKey, TableLookupIndexReqKey> indexNameMapping)
        {
            var message = "Failed at stream " + streamNum + " and instruction " + numInstruction;
            ClassicAssert.AreEqual(expectedPlan.IndexedStream, actualPlan.IndexedStream, message);
            ClassicAssert.AreEqual(expectedPlan.LookupStream, actualPlan.LookupStream, message);
            ClassicAssert.AreEqual(expectedPlan.GetType().FullName, actualPlan.GetType().FullName, message);

            // assert index mapping
            ClassicAssert.AreEqual(expectedPlan.IndexNum.Length, actualPlan.IndexNum.Length, message);
            for (var i = 0; i < expectedPlan.IndexNum.Length; i++) {
                var expectedIndexKey = expectedPlan.IndexNum[i];
                var actualIndexKey = actualPlan.IndexNum[i];
                ClassicAssert.AreEqual(expectedIndexKey, indexNameMapping.Get(actualIndexKey), message);
            }

            if (expectedPlan is FullTableScanLookupPlanForge && actualPlan is FullTableScanLookupPlanForge) {
            }
            else if (expectedPlan is IndexedTableLookupPlanHashedOnlyForge &&
                     actualPlan is IndexedTableLookupPlanHashedOnlyForge) {
                var singleActual = (IndexedTableLookupPlanHashedOnlyForge) actualPlan;
                var singleExpected = (IndexedTableLookupPlanHashedOnlyForge) expectedPlan;
                CompareIndexDesc(singleExpected.KeyDescriptor, singleActual.KeyDescriptor);
            }
            else if (expectedPlan is InKeywordTableLookupPlanMultiIdxForge &&
                     actualPlan is InKeywordTableLookupPlanMultiIdxForge) {
                var inExpected = (InKeywordTableLookupPlanMultiIdxForge) expectedPlan;
                var inActual = (InKeywordTableLookupPlanMultiIdxForge) actualPlan;
                ClassicAssert.IsTrue(ExprNodeUtilityCompare.DeepEquals(inExpected.KeyExpr, inActual.KeyExpr, false));
            }
            else if (expectedPlan is InKeywordTableLookupPlanSingleIdxForge &&
                     actualPlan is InKeywordTableLookupPlanSingleIdxForge) {
                var inExpected = (InKeywordTableLookupPlanSingleIdxForge) expectedPlan;
                var inActual = (InKeywordTableLookupPlanSingleIdxForge) actualPlan;
                ClassicAssert.IsTrue(ExprNodeUtilityCompare.DeepEquals(inExpected.Expressions, inActual.Expressions, false));
            }
            else if (expectedPlan is SortedTableLookupPlanForge && actualPlan is SortedTableLookupPlanForge) {
                var inExpected = (SortedTableLookupPlanForge) expectedPlan;
                var inActual = (SortedTableLookupPlanForge) actualPlan;
                ClassicAssert.AreEqual(inExpected.LookupStream, inActual.LookupStream);
                ClassicAssert.IsTrue(
                    ExprNodeUtilityCompare.DeepEquals(
                        inExpected.RangeKeyPair.Expressions,
                        inActual.RangeKeyPair.Expressions,
                        false));
            }
            else {
                Assert.Fail("Failed to compare plan for stream " + streamNum + ", found type " + actualPlan.GetType());
            }
        }

        private static void CompareIndexDesc(
            TableLookupKeyDesc expected,
            TableLookupKeyDesc actual)
        {
            ClassicAssert.AreEqual(expected.Hashes.Count, actual.Hashes.Count);
            for (var i = 0; i < expected.Hashes.Count; i++) {
                CompareIndexDescHash(expected.Hashes[i], actual.Hashes[i]);
            }

            ClassicAssert.AreEqual(expected.Ranges.Count, actual.Ranges.Count);
            for (var i = 0; i < expected.Ranges.Count; i++) {
                CompareIndexDescRange(expected.Ranges[i], actual.Ranges[i]);
            }
        }

        private static void CompareIndexDescRange(
            QueryGraphValueEntryRangeForge expected,
            QueryGraphValueEntryRangeForge actual)
        {
            ClassicAssert.AreEqual(expected.ToQueryPlan(), actual.ToQueryPlan());
        }

        private static void CompareIndexDescHash(
            QueryGraphValueEntryHashKeyedForge expected,
            QueryGraphValueEntryHashKeyedForge actual)
        {
            ClassicAssert.AreEqual(expected.ToQueryPlan(), actual.ToQueryPlan());
        }

        private static void CompareIndexItem(
            int stream,
            int num,
            QueryPlanIndexItemForge expectedIndex,
            QueryPlanIndexItemForge actualIndex)
        {
            if (!expectedIndex.EqualsCompareSortedProps(actualIndex)) {
                Assert.Fail(
                    "At stream " +
                    stream +
                    " index " +
                    num +
                    "\nExpected:\n" +
                    expectedIndex +
                    "\n" +
                    "Received:\n" +
                    actualIndex +
                    "\n");
            }
        }
    }
} // end of namespace