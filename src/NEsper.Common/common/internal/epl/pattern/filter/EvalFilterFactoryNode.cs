///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.pattern.core;
using com.espertech.esper.common.@internal.filterspec;

namespace com.espertech.esper.common.@internal.epl.pattern.filter
{
    /// <summary>
    ///     This class represents a filter of events in the evaluation tree representing any event expressions.
    /// </summary>
    public class EvalFilterFactoryNode : EvalFactoryNodeBase
    {
        public FilterSpecActivatable FilterSpec { get; set; }

        public string EventAsName { get; set; }

        public int? ConsumptionLevel { get; set; }

        public int EventAsTagNumber { get; set; }

        public override bool IsStateful => false;

        public override bool IsFilterChildNonQuitting => false;

        public override EvalNode MakeEvalNode(
            PatternAgentInstanceContext agentInstanceContext,
            EvalNode parentNode)
        {
            return new EvalFilterNode(agentInstanceContext, this);
        }

        public override void Accept(EvalFactoryNodeVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
} // end of namespace