///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Reflection;

using com.espertech.esper.compat.logging;
using com.espertech.esper.filter;

namespace com.espertech.esper.pattern
{
    /// <summary>
    /// This class represents a filter of events in the evaluation tree representing any event expressions.
    /// </summary>
    public class EvalFilterNode : EvalNodeBase
    {
        public EvalFilterNode(PatternAgentInstanceContext context, EvalFilterFactoryNode factoryNode)
            : base(context)
        {
            FactoryNode = factoryNode;
            if (context.AgentInstanceContext.AgentInstanceFilterProxy != null) {
                AddendumFilters = context.AgentInstanceContext.AgentInstanceFilterProxy.GetAddendumFilters(factoryNode.FilterSpec);
            }
            else {
                AddendumFilters = null;
            }
        }

        public EvalFilterFactoryNode FactoryNode { get; private set; }

        public FilterValueSetParam[][] AddendumFilters { get; private set; }

        public override EvalStateNode NewState(Evaluator parentNode, EvalStateNodeNumber stateNodeNumber, long stateNodeId)
        {
            if (Context.ConsumptionHandler != null) {
                return new EvalFilterStateNodeConsumeImpl(parentNode, this);
            }
            return new EvalFilterStateNode(parentNode, this);
        }
    
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    }
}
