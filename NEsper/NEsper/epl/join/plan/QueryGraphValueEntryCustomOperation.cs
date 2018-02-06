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
    public class QueryGraphValueEntryCustomOperation : QueryGraphValueEntry
    {
        private readonly IDictionary<int, ExprNode> positionalExpressions = new Dictionary<int, ExprNode>();
    
        public IDictionary<int, ExprNode> PositionalExpressions
        {
            get => positionalExpressions;
        }
    }
    
} // end of namespace
