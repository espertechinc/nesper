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
    /// <summary>
    /// Visitor that collects event property identifier information under expression nodes.
    /// </summary>
    public class ExprNodeIdentifierCollectVisitor : ExprNodeVisitor
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        public ExprNodeIdentifierCollectVisitor()
        {
            ExprProperties = new List<ExprIdentNode>();
        }
    
        public bool IsVisit(ExprNode exprNode)
        {
            return true;
        }

        /// <summary>Returns list of event property stream numbers and names that uniquely identify which property is from whcih stream, and the name of each. </summary>
        /// <value>list of event property statement-unique INFO</value>
        public IList<ExprIdentNode> ExprProperties { get; private set; }

        public ICollection<int> StreamsRequired
        {
            get
            {
                ICollection<int> streams = new HashSet<int>();
                foreach (ExprIdentNode node in ExprProperties)
                {
                    streams.Add(node.StreamId);
                }
                return streams;
            }
        }

        public void Visit(ExprNode exprNode)
        {
            var identNode = exprNode as ExprIdentNode;
            if (identNode == null)
            {
                return;
            }

            ExprProperties.Add(identNode);
        }
    }
}
