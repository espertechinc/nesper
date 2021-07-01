///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.compile.stage1.spec
{
    /// <summary>
    /// Specification object to an element in the order-by expression.
    /// </summary>
    [Serializable]
    public class OrderByItem
    {
        public static readonly OrderByItem[] EMPTY_ORDERBY_ARRAY = new OrderByItem[0];

        /// <summary>Ctor. </summary>
        /// <param name="exprNode">is the order-by expression node</param>
        /// <param name="ascending">is true for ascending, or false for descending sort</param>
        public OrderByItem(
            ExprNode exprNode,
            bool ascending)
        {
            ExprNode = exprNode;
            IsDescending = ascending;
        }

        /// <summary>Returns the order-by expression node. </summary>
        /// <value>expression node.</value>
        public ExprNode ExprNode { get; private set; }

        /// <summary>Returns true for ascending, false for descending. </summary>
        /// <value>indicator if ascending or descending</value>
        public bool IsDescending { get; private set; }

        public OrderByItem Copy()
        {
            return new OrderByItem(ExprNode, IsDescending);
        }

        public static OrderByItem[] ToArray(ICollection<OrderByItem> expressions)
        {
            if (expressions.IsEmpty()) {
                return EMPTY_ORDERBY_ARRAY;
            }

            return expressions.ToArray();
        }
    }
}