///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.plugin
{
    public class PlugInAggregationMultiFunctionAgentContext
    {
        public PlugInAggregationMultiFunctionAgentContext(IList<ExprNode> childNodes, ExprNode optionalFilterExpression)
        {
            ChildNodes = childNodes;
            OptionalFilterExpression = optionalFilterExpression;
        }

        public IList<ExprNode> ChildNodes { get; }

        public ExprNode OptionalFilterExpression { get; }
    }
} // end of namespace