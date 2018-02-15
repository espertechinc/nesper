///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.IO;

using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.join.plan;

using NUnit.Framework;

namespace com.espertech.esper.supportregression.epl
{
	public class SupportQueryPlanIndexHelper {

	    public static string GetIndexedExpressions(IDictionary<TableLookupIndexReqKey, QueryPlanIndexItem> entries) {
	        var buf = new StringWriter();
	        foreach (var entry in entries) {
	            buf.Write(entry.Value.IndexProps.Render());
	        }
	        return buf.ToString();
	    }

	    public static void CompareQueryPlans(QueryPlan expectedPlan, QueryPlan actualPlan) {
	        IDictionary<TableLookupIndexReqKey, TableLookupIndexReqKey> indexNameMapping = new Dictionary<TableLookupIndexReqKey, TableLookupIndexReqKey>();
	        CompareIndexes(expectedPlan.IndexSpecs, actualPlan.IndexSpecs, indexNameMapping);
	        CompareExecNodeSpecs(expectedPlan.ExecNodeSpecs, actualPlan.ExecNodeSpecs, indexNameMapping);
	    }

	    private static void CompareIndexes(QueryPlanIndex[] expected, QueryPlanIndex[] actual, IDictionary<TableLookupIndexReqKey, TableLookupIndexReqKey> indexNameMapping) {
	        Assert.AreEqual(expected.Length, actual.Length);
	        for (var i = 0; i < expected.Length; i++) {
	            CompareIndex(i, expected[i], actual[i], indexNameMapping);
	        }
	    }

	    private static void CompareIndex(int streamNum, QueryPlanIndex expected, QueryPlanIndex actual, IDictionary<TableLookupIndexReqKey, TableLookupIndexReqKey> indexNameMapping) {
	        var actualItems = actual.Items;
	        var expectedItems = expected.Items;
            Assert.AreEqual(expectedItems.Count, actualItems.Count, "Number of indexes mismatch for stream " + streamNum);

	        var itActual = actualItems.GetEnumerator();
	        var itExpected = expectedItems.GetEnumerator();

	        var count = 0;
	        while(itActual.MoveNext())
	        {
	            Assert.IsTrue(itExpected.MoveNext());
	            var actualItem = itActual.Current;
	            var expectedItem = itExpected.Current;
	            SupportQueryPlanIndexHelper.CompareIndexItem(streamNum, count, expectedItem.Value, actualItem.Value);
	            count++;
	            indexNameMapping.Put(actualItem.Key, expectedItem.Key);
	        }
	    }

	    private static void CompareExecNodeSpecs(QueryPlanNode[] expected, QueryPlanNode[] actual, IDictionary<TableLookupIndexReqKey, TableLookupIndexReqKey> indexNameMapping) {
	        Assert.AreEqual(expected.Length, actual.Length);
	        for (var i = 0; i < expected.Length; i++) {
	            CompareExecNodeSpec(i, expected[i], actual[i], indexNameMapping);
	        }
	    }

	    private static void CompareExecNodeSpec(int streamNum, QueryPlanNode expected, QueryPlanNode actual, IDictionary<TableLookupIndexReqKey, TableLookupIndexReqKey> indexNameMapping) {
	        if (actual is QueryPlanNodeNoOp && expected == null) {
	        }
	        else if (actual is TableLookupNode && expected is TableLookupNode) {
	            CompareTableLookup(streamNum, (TableLookupNode) expected, (TableLookupNode) actual, indexNameMapping);
	        }
	        else if (actual is TableOuterLookupNode && expected is TableOuterLookupNode) {
	            CompareTableLookupOuter(streamNum, (TableOuterLookupNode) expected, (TableOuterLookupNode) actual, indexNameMapping);
	        }
	        else if (actual is LookupInstructionQueryPlanNode && expected is LookupInstructionQueryPlanNode) {
	            CompareInstruction(streamNum, (LookupInstructionQueryPlanNode) expected, (LookupInstructionQueryPlanNode) actual, indexNameMapping);
	        }
	        else {
	            Assert.Fail("Failed to compare plan node for stream " + streamNum + ", unhandled plan " + actual.GetType().Name);
	        }
	    }

	    private static void CompareInstruction(int streamNum, LookupInstructionQueryPlanNode expected, LookupInstructionQueryPlanNode actual, IDictionary<TableLookupIndexReqKey, TableLookupIndexReqKey> indexNameMapping) {
	        Assert.AreEqual(expected.RootStream, actual.RootStream);
	        Assert.AreEqual(expected.RootStreamName, actual.RootStreamName);
	        Assert.AreEqual(expected.LookupInstructions.Count, actual.LookupInstructions.Count);
	        for (var i = 0; i < expected.LookupInstructions.Count; i++) {
	            CompareInstructionDetail(streamNum, i, expected.LookupInstructions[i], actual.LookupInstructions[i], indexNameMapping);
	        }
	    }

	    private static void CompareInstructionDetail(int streamNum, int numInstruction, LookupInstructionPlan expected, LookupInstructionPlan actual, IDictionary<TableLookupIndexReqKey, TableLookupIndexReqKey> indexNameMapping) {
	        Assert.AreEqual(expected.LookupPlans.Length, actual.LookupPlans.Length);
	        for (var i = 0; i < expected.LookupPlans.Length; i++) {
	            CompareTableLookupPlan(streamNum, numInstruction, expected.LookupPlans[i], actual.LookupPlans[i], indexNameMapping);
	        }
	    }

	    private static void CompareTableLookupOuter(int streamNum, TableOuterLookupNode expected, TableOuterLookupNode actual, IDictionary<TableLookupIndexReqKey, TableLookupIndexReqKey> indexNameMapping) {
	        CompareTableLookupPlan(streamNum, 0, expected.LookupStrategySpec, actual.LookupStrategySpec, indexNameMapping);
	    }

	    private static void CompareTableLookup(int streamNum, TableLookupNode expected, TableLookupNode actual, IDictionary<TableLookupIndexReqKey, TableLookupIndexReqKey> indexNameMapping) {
	        CompareTableLookupPlan(streamNum, 0, expected.TableLookupPlan, actual.TableLookupPlan, indexNameMapping);
	    }

	    private static void CompareTableLookupPlan(int streamNum, int numInstruction, TableLookupPlan expectedPlan, TableLookupPlan actualPlan, IDictionary<TableLookupIndexReqKey, TableLookupIndexReqKey> indexNameMapping) {
	        var message = "Failed at stream " + streamNum + " and instruction " + numInstruction;
	        Assert.AreEqual(expectedPlan.IndexedStream, actualPlan.IndexedStream, message);
	        Assert.AreEqual(expectedPlan.LookupStream, actualPlan.LookupStream, message);
	        Assert.AreEqual(expectedPlan.GetType().Name, actualPlan.GetType().Name, message);

	        // assert index mapping
	        Assert.AreEqual(expectedPlan.IndexNum.Length, actualPlan.IndexNum.Length, message);
	        for (var i = 0; i < expectedPlan.IndexNum.Length; i++) {
	            var expectedIndexKey = expectedPlan.IndexNum[i];
	            var actualIndexKey = actualPlan.IndexNum[i];
	            Assert.AreEqual(expectedIndexKey, indexNameMapping.Get(actualIndexKey), message);
	        }

	        if (expectedPlan is FullTableScanLookupPlan && actualPlan is FullTableScanLookupPlan) {
	        }
	        else if (expectedPlan is IndexedTableLookupPlanSingle && actualPlan is IndexedTableLookupPlanSingle) {
	            var singleActual = (IndexedTableLookupPlanSingle) actualPlan;
	            var singleExpected = (IndexedTableLookupPlanSingle) expectedPlan;
	            CompareIndexDesc(singleExpected.KeyDescriptor, singleActual.KeyDescriptor);
	        }
	        else if (expectedPlan is InKeywordTableLookupPlanMultiIdx && actualPlan is InKeywordTableLookupPlanMultiIdx) {
	            var inExpected = (InKeywordTableLookupPlanMultiIdx) expectedPlan;
	            var inActual = (InKeywordTableLookupPlanMultiIdx) actualPlan;
	            Assert.IsTrue(ExprNodeUtility.DeepEquals(inExpected.KeyExpr, inActual.KeyExpr, false));
	        }
	        else if (expectedPlan is InKeywordTableLookupPlanSingleIdx && actualPlan is InKeywordTableLookupPlanSingleIdx) {
	            var inExpected = (InKeywordTableLookupPlanSingleIdx) expectedPlan;
	            var inActual = (InKeywordTableLookupPlanSingleIdx) actualPlan;
	            Assert.IsTrue(ExprNodeUtility.DeepEquals(inExpected.Expressions, inActual.Expressions, false));
	        }
	        else if (expectedPlan is SortedTableLookupPlan && actualPlan is SortedTableLookupPlan) {
	            var inExpected = (SortedTableLookupPlan) expectedPlan;
	            var inActual = (SortedTableLookupPlan) actualPlan;
	            Assert.AreEqual(inExpected.LookupStream, inActual.LookupStream);
	            Assert.IsTrue(ExprNodeUtility.DeepEquals(inExpected.RangeKeyPair.Expressions, inActual.RangeKeyPair.Expressions, false));
	        }
	        else {
	            Assert.Fail("Failed to compare plan for stream " + streamNum + ", found type " + actualPlan.GetType());
	        }
	    }

	    private static void CompareIndexDesc(TableLookupKeyDesc expected, TableLookupKeyDesc actual) {
	        Assert.AreEqual(expected.Hashes.Count, actual.Hashes.Count);
	        for (var i = 0; i < expected.Hashes.Count; i++) {
	            CompareIndexDescHash(expected.Hashes[i], actual.Hashes[i]);
	        }
	        Assert.AreEqual(expected.Ranges.Count, actual.Ranges.Count);
	        for (var i = 0; i < expected.Ranges.Count; i++) {
	            CompareIndexDescRange(expected.Ranges[i], actual.Ranges[i]);
	        }
	    }

	    private static void CompareIndexDescRange(QueryGraphValueEntryRange expected, QueryGraphValueEntryRange actual) {
	        Assert.AreEqual(expected.ToQueryPlan(), actual.ToQueryPlan());
	    }

	    private static void CompareIndexDescHash(QueryGraphValueEntryHashKeyed expected, QueryGraphValueEntryHashKeyed actual) {
	        Assert.AreEqual(expected.ToQueryPlan(), actual.ToQueryPlan());
	    }

	    private static void CompareIndexItem(int stream, int num, QueryPlanIndexItem expectedIndex, QueryPlanIndexItem actualIndex) {
	        if (!expectedIndex.EqualsCompareSortedProps(actualIndex)) {
	            Assert.Fail("At stream " + stream + " index " + num + "\nExpected:\n" + expectedIndex + "\n" +
	                    "Received:\n" + actualIndex + "\n");
	        }
	    }
	}
} // end of namespace
