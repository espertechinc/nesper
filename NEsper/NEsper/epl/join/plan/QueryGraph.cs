///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.@join.hint;
using com.espertech.esper.type;

namespace com.espertech.esper.epl.join.plan {
    /// <summary>
    ///     Model of relationships between streams based on properties in both streams that are specified
    ///     as equal in a filter expression.
    /// </summary>
    public class QueryGraph {
        public const int SELF_STREAM = int.MinValue;

        private readonly bool _nToZeroAnalysis; // for subqueries and on-action
        private readonly ExcludePlanHint _optionalHint;
        private readonly IDictionary<QueryGraphKey, QueryGraphValue> _streamJoinMap;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="numStreams">number of streams</param>
        /// <param name="optionalHint">the optional hint.</param>
        /// <param name="nToZeroAnalysis">if set to <c>true</c> [n to zero analysis].</param>
        public QueryGraph(int numStreams, ExcludePlanHint optionalHint, bool nToZeroAnalysis) {
            NumStreams = numStreams;
            _optionalHint = optionalHint;
            _nToZeroAnalysis = nToZeroAnalysis;
            _streamJoinMap = new NullableDictionary<QueryGraphKey, QueryGraphValue>();
        }

        /// <summary>Returns the number of streams. </summary>
        /// <value>number of streams</value>
        public int NumStreams { get; }

        /// <summary>
        ///     Add properties for 2 streams that are equal.
        /// </summary>
        /// <param name="streamLeft">left hand stream</param>
        /// <param name="propertyLeft">left hand stream property</param>
        /// <param name="nodeLeft">The node left.</param>
        /// <param name="streamRight">right hand stream</param>
        /// <param name="propertyRight">right hand stream property</param>
        /// <param name="nodeRight">The node right.</param>
        /// <returns>
        ///     true if added and did not exist, false if already known
        /// </returns>
        public bool AddStrictEquals(
            int streamLeft, string propertyLeft, ExprIdentNode nodeLeft, int streamRight,
            string propertyRight, ExprIdentNode nodeRight) {
            Check(streamLeft, streamRight);
            if (propertyLeft == null || propertyRight == null) {
                throw new ArgumentException("Null property names supplied");
            }

            if (streamLeft == streamRight) {
                throw new ArgumentException("Streams supplied are the same");
            }

            var addedLeft = InternalAddEquals(streamLeft, propertyLeft, nodeLeft, streamRight, nodeRight);
            var addedRight = InternalAddEquals(streamRight, propertyRight, nodeRight, streamLeft, nodeLeft);
            return addedLeft || addedRight;
        }

        public bool IsNavigableAtAll(int streamFrom, int streamTo) {
            var key = new QueryGraphKey(streamFrom, streamTo);
            var value = _streamJoinMap.Get(key);
            return value != null && !value.IsEmptyNotNavigable;
        }

        /// <summary>Returns set of streams that the given stream is navigable to. </summary>
        /// <param name="streamFrom">from stream number</param>
        /// <returns>set of streams related to this stream, or empty set if none</returns>
        public ICollection<int> GetNavigableStreams(int streamFrom) {
            ICollection<int> result = new HashSet<int>();
            for (var i = 0; i < NumStreams; i++) {
                if (IsNavigableAtAll(streamFrom, i)) {
                    result.Add(i);
                }
            }

            return result;
        }

        internal QueryGraphValue GetGraphValue(int streamLookup, int streamIndexed) {
            var key = new QueryGraphKey(streamLookup, streamIndexed);
            var value = _streamJoinMap.Get(key);
            if (value != null) {
                return value;
            }

            return new QueryGraphValue();
        }

        /// <summary>
        ///     Fill in equivalent key properties (navigation entries) on all streams. For example, if
        ///     a=b and b=c  then addRelOpInternal a=c. The method adds new equalivalent key properties
        ///     until no additional entries to be added are found, ie. several passes can be made.
        /// </summary>
        /// <param name="typesPerStream">The types per stream.</param>
        /// <param name="queryGraph">navigablity INFO between streamss</param>
        public static void FillEquivalentNav(EventType[] typesPerStream, QueryGraph queryGraph) {
            bool addedEquivalency;

            // Repeat until no more entries were added
            do {
                addedEquivalency = false;

                // For each stream-to-stream combination
                for (var lookupStream = 0; lookupStream < queryGraph.NumStreams; lookupStream++)
                for (var indexedStream = 0; indexedStream < queryGraph.NumStreams; indexedStream++) {
                    if (lookupStream == indexedStream) {
                        continue;
                    }

                    var added = FillEquivalentNav(typesPerStream, queryGraph, lookupStream, indexedStream);
                    if (added) {
                        addedEquivalency = true;
                    }
                }
            } while (addedEquivalency);
        }

        /// <summary>
        /// Looks at the key and index (aka. left and right) properties of the 2 streams and checks
        /// for each property if any equivalent index properties exist for other streams.
        /// </summary>
        /// <param name="typesPerStream">The types per stream.</param>
        /// <param name="queryGraph">The query graph.</param>
        /// <param name="lookupStream">The lookup stream.</param>
        /// <param name="indexedStream">The indexed stream.</param>
        /// <exception cref="IllegalStateException">Unexpected key and index property number mismatch</exception>
        private static bool FillEquivalentNav(
            EventType[] typesPerStream, 
            QueryGraph queryGraph, 
            int lookupStream,
            int indexedStream)
        {
            var addedEquivalency = false;

            var value = queryGraph.GetGraphValue(lookupStream, indexedStream);
            if (value.IsEmptyNotNavigable) {
                return false;
            }

            var hashKeys = value.HashKeyProps;
            var strictKeyProps = hashKeys.StrictKeys;
            var indexProps = hashKeys.Indexed;

            if (strictKeyProps.Count == 0) {
                return false;
            }

            if (strictKeyProps.Count != indexProps.Count) {
                throw new IllegalStateException("Unexpected key and index property number mismatch");
            }

            for (var i = 0; i < strictKeyProps.Count; i++) {
                if (strictKeyProps[i] == null) {
                    continue; // not a strict key
                }

                var added = FillEquivalentNav(
                    typesPerStream, queryGraph, lookupStream, strictKeyProps[i],
                    indexedStream, indexProps[i]);
                if (added) {
                    addedEquivalency = true;
                }
            }

            return addedEquivalency;
        }

        /// <summary>
        ///     Looks at the key and index (aka. left and right) properties of the 2 streams and checks
        ///     for each property if any equivalent index properties exist for other streams.
        ///     Example:  s0.p0 = s1.p1  and  s1.p1 = s2.p2  ==> therefore s0.p0 = s2.p2
        ///     ==> look stream s0, property p0; indexed stream s1, property p1
        ///     Is there any other lookup stream that has stream 1 and property p1 as index property? ==> this is stream s2, p2
        ///     Add navigation entry between stream s0 and property p0 to stream s2, property p2
        /// </summary>
        private static bool FillEquivalentNav(
            EventType[] typesPerStream, QueryGraph queryGraph, int lookupStream,
            string keyProp, int indexedStream, string indexProp) {
            var addedEquivalency = false;

            for (var otherStream = 0; otherStream < queryGraph.NumStreams; otherStream++) {
                if (otherStream == lookupStream || otherStream == indexedStream) {
                    continue;
                }

                var value = queryGraph.GetGraphValue(otherStream, indexedStream);
                var hashKeys = value.HashKeyProps;

                var otherStrictKeyProps = hashKeys.StrictKeys;
                var otherIndexProps = hashKeys.Indexed;
                var otherPropertyNum = -1;

                if (otherIndexProps == null) {
                    continue;
                }

                for (var i = 0; i < otherIndexProps.Count; i++) {
                    if (otherIndexProps[i].Equals(indexProp)) {
                        otherPropertyNum = i;
                        break;
                    }
                }

                if (otherPropertyNum != -1) {
                    if (otherStrictKeyProps[otherPropertyNum] != null) {
                        ExprIdentNode identNodeLookup =
                            new ExprIdentNodeImpl(typesPerStream[lookupStream], keyProp, lookupStream);
                        ExprIdentNode identNodeOther = new ExprIdentNodeImpl(
                            typesPerStream[otherStream],
                            otherStrictKeyProps[otherPropertyNum], otherStream);
                        var added = queryGraph.AddStrictEquals(
                            lookupStream, keyProp, identNodeLookup, otherStream,
                            otherStrictKeyProps[otherPropertyNum], identNodeOther);
                        if (added) {
                            addedEquivalency = true;
                        }
                    }
                }
            }

            return addedEquivalency;
        }

        /// <summary>
        ///     Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        ///     A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString() {
            var writer = new StringWriter();

            var count = 0;
            foreach (var entry in _streamJoinMap) {
                count++;
                writer.WriteLine("Record " + count + ": key=" + entry.Key);
                writer.WriteLine("  value=" + entry.Value);
            }

            return writer.ToString();
        }

        internal void AddRangeStrict(
            int streamNumStart, ExprIdentNode propertyStartExpr,
            int streamNumEnd, ExprIdentNode propertyEndExpr,
            int streamNumValue, ExprIdentNode propertyValueExpr,
            QueryGraphRangeEnum rangeOp) {
            Check(streamNumStart, streamNumValue);
            Check(streamNumEnd, streamNumValue);

            // add as a range if the endpoints are from the same stream
            if (streamNumStart == streamNumEnd && streamNumStart != streamNumValue) {
                InternalAddRange(
                    streamNumStart, streamNumValue, rangeOp, propertyStartExpr, propertyEndExpr,
                    propertyValueExpr);
                InternalAddRelOp(
                    streamNumValue, streamNumStart, propertyValueExpr,
                    QueryGraphRangeEnum.GREATER_OR_EQUAL, propertyEndExpr, false);
                InternalAddRelOp(
                    streamNumValue, streamNumStart, propertyValueExpr, QueryGraphRangeEnum.LESS_OR_EQUAL,
                    propertyStartExpr, false);
            }
            else {
                // endpoints from a different stream, add individually
                if (streamNumValue != streamNumStart) {
                    // read propertyValue >= propertyStart
                    InternalAddRelOp(
                        streamNumStart, streamNumValue, propertyStartExpr,
                        QueryGraphRangeEnum.GREATER_OR_EQUAL, propertyValueExpr, true);
                    // read propertyStart <= propertyValue
                    InternalAddRelOp(
                        streamNumValue, streamNumStart, propertyValueExpr,
                        QueryGraphRangeEnum.LESS_OR_EQUAL, propertyStartExpr, true);
                }

                if (streamNumValue != streamNumEnd) {
                    // read propertyValue <= propertyEnd
                    InternalAddRelOp(
                        streamNumEnd, streamNumValue, propertyEndExpr, QueryGraphRangeEnum.LESS_OR_EQUAL,
                        propertyValueExpr, true);
                    // read propertyEnd >= propertyValue
                    InternalAddRelOp(
                        streamNumValue, streamNumEnd, propertyValueExpr,
                        QueryGraphRangeEnum.GREATER_OR_EQUAL, propertyEndExpr, true);
                }
            }
        }

        public void AddRelationalOpStrict(
            int streamIdLeft, ExprIdentNode propertyLeftExpr,
            int streamIdRight, ExprIdentNode propertyRightExpr,
            RelationalOpEnum relationalOpEnum) {
            Check(streamIdLeft, streamIdRight);
            InternalAddRelOp(
                streamIdLeft, streamIdRight, propertyLeftExpr,
                QueryGraphRangeEnumExtensions.MapFrom(relationalOpEnum.Reversed()), propertyRightExpr, false);
            InternalAddRelOp(
                streamIdRight, streamIdLeft, propertyRightExpr,
                QueryGraphRangeEnumExtensions.MapFrom(relationalOpEnum), propertyLeftExpr, false);
        }

        public void AddUnkeyedExpression(int indexedStream, ExprIdentNode indexedProp, ExprNode exprNodeNoIdent) {
            if (indexedStream < 0 || indexedStream >= NumStreams) {
                throw new ArgumentException("Invalid indexed stream " + indexedStream);
            }

            if (NumStreams > 1) {
                for (var i = 0; i < NumStreams; i++) {
                    if (i != indexedStream) {
                        InternalAddEqualsUnkeyed(i, indexedStream, indexedProp, exprNodeNoIdent);
                    }
                }
            }
            else {
                InternalAddEqualsUnkeyed(SELF_STREAM, indexedStream, indexedProp, exprNodeNoIdent);
            }
        }

        public void AddKeyedExpression(
            int indexedStream, ExprIdentNode indexedProp,
            int keyExprStream, ExprNode exprNodeNoIdent) {
            Check(indexedStream, keyExprStream);
            InternalAddEqualsNoProp(keyExprStream, indexedStream, indexedProp, exprNodeNoIdent);
        }

        private void Check(int indexedStream, int keyStream) {
            if (indexedStream < 0 || indexedStream >= NumStreams) {
                throw new ArgumentException("Invalid indexed stream " + indexedStream);
            }

            if (keyStream >= NumStreams) {
                throw new ArgumentException("Invalid key stream " + keyStream);
            }

            if (NumStreams > 1) {
                if (keyStream < 0) {
                    throw new ArgumentException("Invalid key stream " + keyStream);
                }
            }
            else {
                if (keyStream != SELF_STREAM) {
                    throw new ArgumentException("Invalid key stream " + keyStream);
                }
            }

            if (keyStream == indexedStream) {
                throw new ArgumentException("Invalid key stream equals indexed stream " + keyStream);
            }
        }

        public void AddRangeExpr(
            int indexedStream,
            ExprIdentNode indexedProp,
            ExprNode startNode, int? optionalStartStreamNum,
            ExprNode endNode, int? optionalEndStreamNum,
            QueryGraphRangeEnum rangeOp) {
            if (optionalStartStreamNum == null && optionalEndStreamNum == null) {
                if (NumStreams > 1) {
                    for (var i = 0; i < NumStreams; i++) {
                        if (i == indexedStream) {
                            continue;
                        }

                        InternalAddRange(i, indexedStream, rangeOp, startNode, endNode, indexedProp);
                    }
                }
                else {
                    InternalAddRange(SELF_STREAM, indexedStream, rangeOp, startNode, endNode, indexedProp);
                }

                return;
            }

            optionalStartStreamNum = optionalStartStreamNum ?? -1;
            optionalEndStreamNum = optionalEndStreamNum ?? -1;

            // add for a specific stream only
            if (optionalStartStreamNum.Equals(optionalEndStreamNum) || optionalEndStreamNum.Equals(-1)) {
                InternalAddRange(optionalStartStreamNum.Value, indexedStream, rangeOp, startNode, endNode, indexedProp);
            }

            if (optionalStartStreamNum.Equals(-1)) {
                InternalAddRange(optionalEndStreamNum.Value, indexedStream, rangeOp, startNode, endNode, indexedProp);
            }
        }

        public void AddRelationalOp(
            int indexedStream,
            ExprIdentNode indexedProp,
            int? keyStreamNum,
            ExprNode exprNodeNoIdent,
            RelationalOpEnum relationalOpEnum) {
            if (keyStreamNum == null) {
                if (NumStreams > 1) {
                    for (var i = 0; i < NumStreams; i++) {
                        if (i == indexedStream) {
                            continue;
                        }

                        InternalAddRelOp(
                            i, indexedStream, exprNodeNoIdent,
                            QueryGraphRangeEnumExtensions.MapFrom(relationalOpEnum), indexedProp, false);
                    }
                }
                else {
                    InternalAddRelOp(
                        SELF_STREAM, indexedStream, exprNodeNoIdent,
                        QueryGraphRangeEnumExtensions.MapFrom(relationalOpEnum), indexedProp, false);
                }

                return;
            }

            // add for a specific stream only
            InternalAddRelOp(
                keyStreamNum.Value, indexedStream, exprNodeNoIdent,
                QueryGraphRangeEnumExtensions.MapFrom(relationalOpEnum), indexedProp, false);
        }

        public void AddInSetSingleIndex(
            int testStreamNum, ExprNode testPropExpr,
            int setStreamNum, ExprNode[] setPropExpr) {
            Check(testStreamNum, setStreamNum);
            InternalAddInKeywordSingleIndex(setStreamNum, testStreamNum, testPropExpr, setPropExpr);
        }

        public void AddInSetSingleIndexUnkeyed(
            int testStreamNum,
            ExprNode testPropExpr,
            ExprNode[] setPropExpr) {
            if (NumStreams > 1) {
                for (var i = 0; i < NumStreams; i++) {
                    if (i != testStreamNum) {
                        InternalAddInKeywordSingleIndex(i, testStreamNum, testPropExpr, setPropExpr);
                    }
                }
            }
            else {
                InternalAddInKeywordSingleIndex(SELF_STREAM, testStreamNum, testPropExpr, setPropExpr);
            }
        }

        public void AddInSetMultiIndex(
            int testStreamNum, ExprNode testPropExpr,
            int setStreamNum, ExprNode[] setPropExpr) {
            Check(testStreamNum, setStreamNum);
            InternalAddInKeywordMultiIndex(testStreamNum, setStreamNum, testPropExpr, setPropExpr);
        }

        public void AddInSetMultiIndexUnkeyed(
            ExprNode testPropExpr,
            int setStreamNum,
            ExprNode[] setPropExpr) {
            for (var i = 0; i < NumStreams; i++) {
                if (i != setStreamNum) {
                    InternalAddInKeywordMultiIndex(i, setStreamNum, testPropExpr, setPropExpr);
                }
            }
        }

        public void AddCustomIndex(
            string operationName,
            IList<ExprNode> indexExpressions,
            IList<Pair<ExprNode, int[]>> streamKeys,
            int streamValue) {
            var expressionPosition = 0;
            foreach (var pair in streamKeys) {
                if (pair.Second.Length == 0) {
                    if (NumStreams > 1) {
                        for (var i = 0; i < NumStreams; i++) {
                            var value = GetCreateValue(i, streamValue);
                            value.AddCustom(indexExpressions, operationName, expressionPosition, pair.First);
                        }
                    }
                    else {
                        var value = GetCreateValue(SELF_STREAM, streamValue);
                        value.AddCustom(indexExpressions, operationName, expressionPosition, pair.First);
                    }
                }
                else {
                    foreach (var providingStream in pair.Second) {
                        var value = GetCreateValue(providingStream, streamValue);
                        value.AddCustom(indexExpressions, operationName, expressionPosition, pair.First);
                    }
                }

                expressionPosition++;
            }
        }

        private void InternalAddRange(
            int streamKey, int streamValue, QueryGraphRangeEnum rangeOp,
            ExprNode propertyStartExpr,
            ExprNode propertyEndExpr,
            ExprIdentNode propertyValueExpr) {
            if (_nToZeroAnalysis && streamValue != 0) {
                return;
            }

            if (_optionalHint != null &&
                _optionalHint.Filter(streamKey, streamValue, ExcludePlanFilterOperatorType.RELOP)) {
                return;
            }

            var valueLeft = GetCreateValue(streamKey, streamValue);
            valueLeft.AddRange(rangeOp, propertyStartExpr, propertyEndExpr, propertyValueExpr);
        }

        private void InternalAddRelOp(
            int streamKey, int streamValue,
            ExprNode keyExpr, QueryGraphRangeEnum rangeEnum,
            ExprIdentNode valueExpr, bool isBetweenOrIn) {
            if (_nToZeroAnalysis && streamValue != 0) {
                return;
            }

            if (_optionalHint != null &&
                _optionalHint.Filter(streamKey, streamValue, ExcludePlanFilterOperatorType.RELOP)) {
                return;
            }

            var value = GetCreateValue(streamKey, streamValue);
            value.AddRelOp(keyExpr, rangeEnum, valueExpr, isBetweenOrIn);
        }

        private bool InternalAddEquals(
            int streamLookup, string propertyLookup, ExprIdentNode propertyLookupNode,
            int streamIndexed, ExprIdentNode propertyIndexedNode) {
            if (_nToZeroAnalysis && streamIndexed != 0) {
                return false;
            }

            if (_optionalHint != null && _optionalHint.Filter(
                    streamLookup, propertyIndexedNode.StreamId,
                    ExcludePlanFilterOperatorType.EQUALS, propertyLookupNode, propertyIndexedNode)) {
                return false;
            }

            var value = GetCreateValue(streamLookup, streamIndexed);
            return value.AddStrictCompare(propertyLookup, propertyLookupNode, propertyIndexedNode);
        }

        private void InternalAddEqualsNoProp(
            int keyExprStream, int indexedStream,
            ExprIdentNode indexedProp,
            ExprNode exprNodeNoIdent) {
            if (_nToZeroAnalysis && indexedStream != 0) {
                return;
            }

            if (_optionalHint != null &&
                _optionalHint.Filter(keyExprStream, indexedStream, ExcludePlanFilterOperatorType.EQUALS)) {
                return;
            }

            var value = GetCreateValue(keyExprStream, indexedStream);
            value.AddKeyedExpr(indexedProp, exprNodeNoIdent);
        }

        private void InternalAddEqualsUnkeyed(
            int streamKey, int streamValue,
            ExprIdentNode indexedProp,
            ExprNode exprNodeNoIdent) {
            if (_nToZeroAnalysis && streamValue != 0) {
                return;
            }

            if (_optionalHint != null &&
                _optionalHint.Filter(streamKey, streamValue, ExcludePlanFilterOperatorType.EQUALS)) {
                return;
            }

            var value = GetCreateValue(streamKey, streamValue);
            value.AddUnkeyedExpr(indexedProp, exprNodeNoIdent);
        }

        private void InternalAddInKeywordSingleIndex(
            int streamKey, int streamValue,
            ExprNode testPropExpr,
            ExprNode[] setPropExpr) {
            if (_nToZeroAnalysis && streamValue != 0) {
                return;
            }

            if (_optionalHint != null &&
                _optionalHint.Filter(streamKey, streamValue, ExcludePlanFilterOperatorType.INKW)) {
                return;
            }

            var valueSingleIdx = GetCreateValue(streamKey, streamValue);
            valueSingleIdx.AddInKeywordSingleIdx(testPropExpr, setPropExpr);
        }

        private void InternalAddInKeywordMultiIndex(
            int streamKey, int streamValue,
            ExprNode testPropExpr,
            ExprNode[] setPropExpr) {
            if (_nToZeroAnalysis && streamValue != 0) {
                return;
            }

            if (_optionalHint != null &&
                _optionalHint.Filter(streamKey, streamValue, ExcludePlanFilterOperatorType.INKW)) {
                return;
            }

            var value = GetCreateValue(streamKey, streamValue);
            value.AddInKeywordMultiIdx(testPropExpr, setPropExpr);
        }

        private QueryGraphValue GetCreateValue(int streamKey, int streamValue) {
            Check(streamValue, streamKey);
            var key = new QueryGraphKey(streamKey, streamValue);
            var value = _streamJoinMap.Get(key);
            if (value == null) {
                value = new QueryGraphValue();
                _streamJoinMap.Put(key, value);
            }

            return value;
        }
    }
}