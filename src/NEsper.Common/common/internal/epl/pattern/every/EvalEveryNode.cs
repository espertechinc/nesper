///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.pattern.core;

namespace com.espertech.esper.common.@internal.epl.pattern.every
{
    /// <summary>
    ///     This class represents an 'every' operator in the evaluation tree representing an event expression.
    /// </summary>
    public class EvalEveryNode : EvalNodeBase
    {
        internal readonly EvalEveryFactoryNode factoryNode;

        public EvalEveryNode(
            PatternAgentInstanceContext context,
            EvalEveryFactoryNode factoryNode,
            EvalNode childNode)
            : base(context)
        {
            this.factoryNode = factoryNode;
            ChildNode = childNode;
        }

        public EvalEveryFactoryNode FactoryNode => factoryNode;

        public EvalNode ChildNode { get; }

        public override EvalStateNode NewState(Evaluator parentNode)
        {
            return new EvalEveryStateNode(parentNode, this);
        }
    }
} // end of namespace