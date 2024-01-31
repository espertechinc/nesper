///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.filterspec;

namespace com.espertech.esper.common.@internal.epl.namedwindow.consume
{
    public class NamedWindowConsumerDesc
    {
        private readonly int namedWindowConsumerId;
        private readonly ExprEvaluator filterEvaluator;
        private readonly PropertyEvaluator optPropertyEvaluator;
        private readonly AgentInstanceContext agentInstanceContext;

        public NamedWindowConsumerDesc(
            int namedWindowConsumerId,
            ExprEvaluator filterEvaluator,
            PropertyEvaluator optPropertyEvaluator,
            AgentInstanceContext agentInstanceContext)
        {
            this.namedWindowConsumerId = namedWindowConsumerId;
            this.filterEvaluator = filterEvaluator;
            this.optPropertyEvaluator = optPropertyEvaluator;
            this.agentInstanceContext = agentInstanceContext;
        }

        public int NamedWindowConsumerId => namedWindowConsumerId;

        public ExprEvaluator FilterEvaluator => filterEvaluator;

        public PropertyEvaluator OptPropertyEvaluator => optPropertyEvaluator;

        public AgentInstanceContext AgentInstanceContext => agentInstanceContext;
    }
} // end of namespace