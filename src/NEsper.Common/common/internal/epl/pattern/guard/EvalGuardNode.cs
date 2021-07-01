///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.pattern.core;

namespace com.espertech.esper.common.@internal.epl.pattern.guard
{
    /// <summary>
    ///     This class represents a guard in the evaluation tree representing an event expressions.
    /// </summary>
    public class EvalGuardNode : EvalNodeBase
    {
        internal readonly EvalGuardFactoryNode factoryNode;

        public EvalGuardNode(
            PatternAgentInstanceContext context,
            EvalGuardFactoryNode factoryNode,
            EvalNode childNode)
            : base(context)
        {
            this.factoryNode = factoryNode;
            ChildNode = childNode;
        }

        public EvalGuardFactoryNode FactoryNode => factoryNode;

        public EvalNode ChildNode { get; }

        public override EvalStateNode NewState(Evaluator parentNode)
        {
            return new EvalGuardStateNode(parentNode, this);
        }
    }
} // end of namespace