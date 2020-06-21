///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.epl.expression.core
{
    public class ExprNodeWithParentPair
    {
        public ExprNodeWithParentPair(
            ExprNode node,
            ExprNode parent)
        {
            Node = node;
            Parent = parent;
        }

        public ExprNode Node { get; }

        public ExprNode Parent { get; }
    }
} // end of namespace