///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.expression.visitor
{
    public class ExprNodeStreamRequiredVisitor : ExprNodeVisitor
    {
        private readonly ISet<int> _streams;

        public ExprNodeStreamRequiredVisitor()
        {
            _streams = new HashSet<int>();
        }

        public bool IsVisit(ExprNode exprNode)
        {
            return true;
        }

        public void Visit(ExprNode exprNode)
        {
            if (exprNode is ExprStreamRefNode)
            {
                var streamRefNode = (ExprStreamRefNode) exprNode;
                int? streamRef = streamRefNode.StreamReferencedIfAny;
                if (streamRef != null)
                {
                    _streams.Add(streamRef.Value);
                }
            }
        }

        public ISet<int> StreamsRequired
        {
            get { return _streams; }
        }
    }
} // end of namespace