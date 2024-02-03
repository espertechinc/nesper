///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.pattern.core;

namespace com.espertech.esper.common.@internal.epl.pattern.matchuntil
{
    /// <summary>
    ///     This class represents a match-until observer in the evaluation tree representing any event expressions.
    /// </summary>
    public class EvalMatchUntilNode : EvalNodeBase
    {
        internal readonly EvalMatchUntilFactoryNode factoryNode;

        public EvalMatchUntilNode(
            PatternAgentInstanceContext context,
            EvalMatchUntilFactoryNode factoryNode,
            EvalNode childNodeSub,
            EvalNode childNodeUntil)
            : base(context)
        {
            this.factoryNode = factoryNode;
            ChildNodeSub = childNodeSub;
            ChildNodeUntil = childNodeUntil;
        }

        public EvalMatchUntilFactoryNode FactoryNode => factoryNode;

        public EvalNode ChildNodeSub { get; }

        public EvalNode ChildNodeUntil { get; }

        public override EvalStateNode NewState(Evaluator parentNode)
        {
            return new EvalMatchUntilStateNode(parentNode, this);
        }
    }
} // end of namespace