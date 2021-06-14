///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.pattern.core;

namespace com.espertech.esper.common.@internal.epl.pattern.everydistinct
{
    /// <summary>
    ///     This class represents an 'every-distinct' operator in the evaluation tree representing an event expression.
    /// </summary>
    public class EvalEveryDistinctNode : EvalNodeBase
    {
        internal readonly EvalEveryDistinctFactoryNode factoryNode;

        public EvalEveryDistinctNode(
            EvalEveryDistinctFactoryNode factoryNode,
            EvalNode childNode,
            PatternAgentInstanceContext agentInstanceContext)
            : base(agentInstanceContext)
        {
            this.factoryNode = factoryNode;
            ChildNode = childNode;
        }

        public EvalEveryDistinctFactoryNode FactoryNode => factoryNode;

        public EvalNode ChildNode { get; }

        public override EvalStateNode NewState(Evaluator parentNode)
        {
            if (factoryNode.TimePeriodCompute == null) {
                return new EvalEveryDistinctStateNode(parentNode, this);
            }

            return new EvalEveryDistinctStateExpireKeyNode(parentNode, this);
        }
    }
} // end of namespace