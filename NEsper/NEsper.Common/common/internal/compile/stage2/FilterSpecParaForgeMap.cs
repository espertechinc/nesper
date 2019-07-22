///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.compile.stage2
{
    /// <summary>
    ///     A two-sided map for filter parameters mapping filter expression nodes to filter parameters and
    ///     back. For use in optimizing filter expressions.
    /// </summary>
    public class FilterSpecParaForgeMap
    {
        private readonly IDictionary<ExprNode, FilterSpecParamForge> exprNodes;
        private readonly IDictionary<FilterSpecParamForge, ExprNode> specParams;

        /// <summary>
        ///     Ctor.
        /// </summary>
        public FilterSpecParaForgeMap()
        {
            exprNodes = new LinkedHashMap<ExprNode, FilterSpecParamForge>();
            specParams = new LinkedHashMap<FilterSpecParamForge, ExprNode>();
        }

        /// <summary>
        ///     Returns all expression nodes for which no filter parameter exists.
        /// </summary>
        /// <value>list of expression nodes</value>
        public IList<ExprNode> UnassignedExpressions {
            get {
                IList<ExprNode> unassigned = new List<ExprNode>();
                foreach (var entry in exprNodes) {
                    if (entry.Value == null) {
                        unassigned.Add(entry.Key);
                    }
                }

                return unassigned;
            }
        }

        /// <summary>
        ///     Returns all filter parameters.
        /// </summary>
        /// <value>filter parameters</value>
        public ICollection<FilterSpecParamForge> FilterParams => specParams.Keys;

        /// <summary>
        ///     Add a node and filter param.
        /// </summary>
        /// <param name="exprNode">is the node to add</param>
        /// <param name="param">is null if the expression node has not optimized form</param>
        public void Put(
            ExprNode exprNode,
            FilterSpecParamForge param)
        {
            exprNodes.Put(exprNode, param);
            if (param != null) {
                specParams.Put(param, exprNode);
            }
        }

        public int CountUnassignedExpressions()
        {
            var count = 0;
            foreach (var entry in exprNodes) {
                if (entry.Value == null) {
                    count++;
                }
            }

            return count;
        }

        public void RemoveNode(ExprNode node)
        {
            FilterSpecParamForge param = exprNodes.Delete(node);
            if (param != null) {
                specParams.Remove(param);
            }
        }

        /// <summary>
        ///     Removes a filter parameter and it's associated expression node
        /// </summary>
        /// <param name="param">is the parameter to remove</param>
        /// <returns>expression node removed</returns>
        public ExprNode RemoveEntry(FilterSpecParamForge param)
        {
            var exprNode = specParams.Get(param);
            if (exprNode == null) {
                throw new IllegalStateException("Not found in collection param: " + param);
            }

            specParams.Remove(param);
            exprNodes.Remove(exprNode);

            return exprNode;
        }

        /// <summary>
        ///     Remove a filter parameter leaving the expression node in place.
        /// </summary>
        /// <param name="param">filter parameter to remove</param>
        public void RemoveValue(FilterSpecParamForge param)
        {
            var exprNode = specParams.Get(param);
            if (exprNode == null) {
                throw new IllegalStateException("Not found in collection param: " + param);
            }

            specParams.Remove(param);
            exprNodes.Put(exprNode, null);
        }

        public void Clear()
        {
            exprNodes.Clear();
            specParams.Clear();
        }

        public void Add(FilterSpecParaForgeMap other)
        {
            exprNodes.PutAll(other.exprNodes);
            specParams.PutAll(other.specParams);
        }
    }
} // end of namespace