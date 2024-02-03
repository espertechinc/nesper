///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.epl.expression.visitor
{
    public class ExprNodeStreamRequiredVisitor : ExprNodeVisitor
    {
        public ExprNodeStreamRequiredVisitor()
        {
            StreamsRequired = new HashSet<int>();
        }

        public bool IsWalkDeclExprParam => true;

        public ISet<int> StreamsRequired { get; }

        public bool IsVisit(ExprNode exprNode)
        {
            return true;
        }

        public void Visit(ExprNode exprNode)
        {
            if (exprNode is ExprStreamRefNode streamRefNode) {
                var streamRef = streamRefNode.StreamReferencedIfAny;
                if (streamRef != null) {
                    StreamsRequired.Add(streamRef.Value);
                }
            }
        }
    }
} // end of namespace