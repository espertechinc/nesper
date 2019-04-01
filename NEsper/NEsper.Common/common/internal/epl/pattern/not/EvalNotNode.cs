///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.pattern.core;

namespace com.espertech.esper.common.@internal.epl.pattern.not
{
    /// <summary>
    ///     This class represents an 'not' operator in the evaluation tree representing any event expressions.
    /// </summary>
    public class EvalNotNode : EvalNodeBase
    {
        internal readonly EvalNotFactoryNode factoryNode;

        public EvalNotNode(
            PatternAgentInstanceContext context, EvalNotFactoryNode factoryNode, EvalNode childNode) : base(context)
        {
            this.factoryNode = factoryNode;
            ChildNode = childNode;
        }

        public EvalNotFactoryNode FactoryNode => factoryNode;

        public EvalNode ChildNode { get; }

        public override EvalStateNode NewState(Evaluator parentNode)
        {
            return new EvalNotStateNode(parentNode, this);
        }
    }
} // end of namespace