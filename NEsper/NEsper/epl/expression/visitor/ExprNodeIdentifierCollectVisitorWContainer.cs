///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.collection;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.expression.visitor
{
    /// <summary>
    /// Visitor that collects event property identifier information under expression nodes.
    /// </summary>
    [Serializable]
    public class ExprNodeIdentifierCollectVisitorWContainer : ExprNodeVisitorWithParent
    {
        private readonly IList<Pair<ExprNode, ExprIdentNode>> _exprProperties;
    
        /// <summary>Ctor. </summary>
        public ExprNodeIdentifierCollectVisitorWContainer()
        {
            _exprProperties = new List<Pair<ExprNode, ExprIdentNode>>(2);
        }
    
        public bool IsVisit(ExprNode exprNode)
        {
            return true;
        }

        /// <summary>
        /// Returns list of event property stream numbers and names that uniquely identify which property is from whcih stream, and the name of each.
        /// </summary>
        /// <value>list of event property statement-unique info</value>
        public IList<Pair<ExprNode, ExprIdentNode>> ExprProperties
        {
            get { return _exprProperties; }
        }

        public void Visit(ExprNode exprNode, ExprNode containerExprNode)
        {
            var identNode = exprNode as ExprIdentNode;
            if (identNode != null)
            {
                _exprProperties.Add(new Pair<ExprNode, ExprIdentNode>(containerExprNode, identNode));
            }
        }
    }
}
