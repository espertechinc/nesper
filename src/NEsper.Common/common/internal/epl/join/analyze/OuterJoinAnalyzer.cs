///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.join.querygraph;
using com.espertech.esper.common.@internal.type;

namespace com.espertech.esper.common.@internal.epl.join.analyze
{
    /// <summary>
    ///     Analyzes an outer join descriptor list and builds a query graph model from it.
    ///     The 'on' expression identifiers are extracted
    ///     and placed in the query graph model as navigable relationships (by key and index
    ///     properties) between streams.
    /// </summary>
    public class OuterJoinAnalyzer
    {
        /// <summary>
        ///     Analyzes the outer join descriptor list to build a query graph model.
        /// </summary>
        /// <param name="outerJoinDescList">list of outer join descriptors</param>
        /// <param name="queryGraph">model containing relationships between streams that is written into</param>
        /// <returns>filterQueryGraph object</returns>
        public static QueryGraphForge Analyze(
            OuterJoinDesc[] outerJoinDescList,
            QueryGraphForge queryGraph)
        {
            foreach (var outerJoinDesc in outerJoinDescList) {
                // add optional on-expressions
                if (outerJoinDesc.OptLeftNode != null) {
                    var identNodeLeft = outerJoinDesc.OptLeftNode;
                    var identNodeRight = outerJoinDesc.OptRightNode;

                    Add(queryGraph, identNodeLeft, identNodeRight);

                    if (outerJoinDesc.AdditionalLeftNodes != null) {
                        for (var i = 0; i < outerJoinDesc.AdditionalLeftNodes.Length; i++) {
                            Add(
                                queryGraph,
                                outerJoinDesc.AdditionalLeftNodes[i],
                                outerJoinDesc.AdditionalRightNodes[i]);
                        }
                    }
                }
            }

            return queryGraph;
        }

        private static void Add(
            QueryGraphForge queryGraph,
            ExprIdentNode identNodeLeft,
            ExprIdentNode identNodeRight)
        {
            queryGraph.AddStrictEquals(
                identNodeLeft.StreamId,
                identNodeLeft.ResolvedPropertyName,
                identNodeLeft,
                identNodeRight.StreamId,
                identNodeRight.ResolvedPropertyName,
                identNodeRight);
        }

        public static bool OptionalStreamsIfAny(IList<OuterJoinDesc> outerJoinDescList)
        {
            if (outerJoinDescList == null || outerJoinDescList.Count == 0) {
                return false;
            }

            foreach (var outerJoinDesc in outerJoinDescList) {
                if (outerJoinDesc.OuterJoinType != OuterJoinType.INNER) {
                    return true;
                }
            }

            return false;
        }
    }
} // end of namespace