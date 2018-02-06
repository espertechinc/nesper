///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.join.plan
{
    public class QueryGraphValueEntryCustomKey : QueryGraphValueEntry
    {
        private readonly string _operationName;
        private readonly IList<ExprNode> _exprNodes;

        public QueryGraphValueEntryCustomKey(string operationName, IList<ExprNode> exprNodes)
        {
            _operationName = operationName;
            _exprNodes = exprNodes;
        }

        public string OperationName => _operationName;

        public IList<ExprNode> ExprNodes => _exprNodes;

        public bool Equals(QueryGraphValueEntryCustomKey other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other._operationName, _operationName) &&
                ExprNodeUtility.DeepEquals(_exprNodes, other._exprNodes, true);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(QueryGraphValueEntryCustomKey)) return false;
            return Equals((QueryGraphValueEntryCustomKey) obj);
        }
    
        public override int GetHashCode()
        {
            return _operationName.GetHashCode();
        }
    }
    
} // end of namespace
