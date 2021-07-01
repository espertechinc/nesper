///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.epl.resultset.order
{
    public class OrderByElementForge
    {
        private ExprNode exprNode;
        private bool isDescending;

        public OrderByElementForge(
            ExprNode exprNode,
            bool isDescending)
        {
            this.exprNode = exprNode;
            this.isDescending = isDescending;
        }

        public ExprNode ExprNode {
            get => exprNode;
        }

        public bool IsDescending()
        {
            return isDescending;
        }
    }
} // end of namespace