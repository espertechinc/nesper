///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;

namespace com.espertech.esper.filter
{
    /// <summary>
    /// A two-sided map for filter parameters mapping filter expression nodes to filter 
    /// parameters and back. For use in optimizing filter expressions.
    /// </summary>
    public class FilterParamExprMap
    {
        private readonly IDictionary<ExprNode, FilterSpecParam> _exprNodes;
        private readonly IDictionary<FilterSpecParam, ExprNode> _specParams;
    
        /// <summary>Ctor. </summary>
        public FilterParamExprMap()
        {
            _exprNodes = new LinkedHashMap<ExprNode, FilterSpecParam>();
            _specParams = new LinkedHashMap<FilterSpecParam, ExprNode>();
        }
    
        /// <summary>Add a node and filter param. </summary>
        /// <param name="exprNode">is the node to add</param>
        /// <param name="param">is null if the expression node has not optimized form</param>
        public void Put(ExprNode exprNode, FilterSpecParam param)
        {
            _exprNodes.Put(exprNode, param);
            if (param != null)
            {
                _specParams.Put(param, exprNode);
            }
        }

        /// <summary>Returns all expression nodes for which no filter parameter exists. </summary>
        /// <value>list of expression nodes</value>
        public IList<ExprNode> UnassignedExpressions
        {
            get
            {
                return _exprNodes.Where(entry => entry.Value == null).Select(entry => entry.Key).ToList();
            }
        }

        public int CountUnassignedExpressions()
        {
            return _exprNodes.Where(entry => entry.Value == null).Select(entry => entry.Key).Count();
        }

        /// <summary>Returns all filter parameters. </summary>
        /// <value>filter parameters</value>
        public ICollection<FilterSpecParam> FilterParams
        {
            get { return _specParams.Keys; }
        }

        public void RemoveNode(ExprNode node)
        {
            var param = _exprNodes.Pluck(node);
            if (param != null)
            {
                _specParams.Remove(param);
            }
        }

        /// <summary>Removes a filter parameter and it's associated expression node </summary>
        /// <param name="param">is the parameter to remove</param>
        /// <returns>expression node removed</returns>
        public ExprNode RemoveEntry(FilterSpecParam param)
        {
            ExprNode exprNode = _specParams.Get(param);
            if (exprNode == null)
            {
                throw new IllegalStateException("Not found in collection param: " + param);
            }
    
            _specParams.Remove(param);
            _exprNodes.Remove(exprNode);
    
            return exprNode;
        }
    
        /// <summary>Remove a filter parameter leaving the expression node in place. </summary>
        /// <param name="param">filter parameter to remove</param>
        public void RemoveValue(FilterSpecParam param)
        {
            ExprNode exprNode = _specParams.Get(param);
            if (exprNode == null)
            {
                throw new IllegalStateException("Not found in collection param: " + param);
            }
    
            _specParams.Remove(param);
            _exprNodes.Put(exprNode, null);
        }

        public void Clear()
        {
            _exprNodes.Clear();
            _specParams.Clear();
        }

        public void Add(FilterParamExprMap other)
        {
            _exprNodes.PutAll(other._exprNodes);
            _specParams.PutAll(other._specParams);
        }
    }
}
