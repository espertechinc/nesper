///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.property;

namespace com.espertech.esper.epl.named
{
    public class NamedWindowConsumerDesc
    {
        public NamedWindowConsumerDesc(IList<ExprNode> filterList, PropertyEvaluator optPropertyEvaluator, AgentInstanceContext agentInstanceContext) {
            FilterList = filterList;
            OptPropertyEvaluator = optPropertyEvaluator;
            AgentInstanceContext = agentInstanceContext;
        }

        public IList<ExprNode> FilterList { get; private set; }

        public PropertyEvaluator OptPropertyEvaluator { get; private set; }

        public AgentInstanceContext AgentInstanceContext { get; private set; }
    }
}
