///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.spec;

namespace com.espertech.esper.epl.join.plan
{
    /// <summary>Analyzes an outer join descriptor list and builds a query graph model from it. The 'on' expression identifiers are extracted and placed in the query graph model as navigable relationships (by key and index properties) between streams. </summary>
    public class OuterJoinAnalyzer
    {
        /// <summary>Analyzes the outer join descriptor list to build a query graph model. </summary>
        /// <param name="outerJoinDescList">list of outer join descriptors</param>
        /// <param name="queryGraph">model containing relationships between streams that is written into</param>
        /// <returns>queryGraph object</returns>
        public static QueryGraph Analyze(OuterJoinDesc[] outerJoinDescList, QueryGraph queryGraph)
        {
            foreach (OuterJoinDesc outerJoinDesc in outerJoinDescList)
            {
                // add optional on-expressions
                if (outerJoinDesc.OptLeftNode != null) {
                    ExprIdentNode identNodeLeft = outerJoinDesc.OptLeftNode;
                    ExprIdentNode identNodeRight = outerJoinDesc.OptRightNode;
    
                    Add(queryGraph, identNodeLeft, identNodeRight);
    
                    if (outerJoinDesc.AdditionalLeftNodes != null)
                    {
                        for (int i = 0; i < outerJoinDesc.AdditionalLeftNodes.Length; i++)
                        {
                            Add(queryGraph, outerJoinDesc.AdditionalLeftNodes[i], outerJoinDesc.AdditionalRightNodes[i]);
                        }
                    }
                }
                else {
    
                }
            }
    
            return queryGraph;
        }
    
        private static void Add(QueryGraph queryGraph, ExprIdentNode identNodeLeft, ExprIdentNode identNodeRight)
        {
            queryGraph.AddStrictEquals(identNodeLeft.StreamId, identNodeLeft.ResolvedPropertyName, identNodeLeft,
                    identNodeRight.StreamId, identNodeRight.ResolvedPropertyName, identNodeRight);
        }
    }
}
