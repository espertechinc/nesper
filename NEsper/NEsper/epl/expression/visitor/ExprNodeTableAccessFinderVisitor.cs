///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.table;

namespace com.espertech.esper.epl.expression.visitor
{
    public class ExprNodeTableAccessFinderVisitor : ExprNodeVisitor
    {
        private bool _hasTableAccess;

        public bool HasTableAccess
        {
            get { return _hasTableAccess; }
        }

        public bool IsVisit(ExprNode exprNode)
        {
            return !_hasTableAccess;
        }

        public void Visit(ExprNode exprNode)
        {
            if (exprNode is ExprTableAccessNode)
            {
                _hasTableAccess = true;
            }
        }
    }
}