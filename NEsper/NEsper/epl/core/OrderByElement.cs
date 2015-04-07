///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;

namespace com.espertech.esper.epl.core
{
    public class OrderByElement
    {
        public OrderByElement(ExprNode exprNode, ExprEvaluator expr, bool descending)
        {
            ExprNode = exprNode;
            Expr = expr;
            IsDescending = descending;
        }

        public ExprNode ExprNode { get; private set; }

        public ExprEvaluator Expr { get; private set; }

        public bool IsDescending { get; private set; }
    }
}