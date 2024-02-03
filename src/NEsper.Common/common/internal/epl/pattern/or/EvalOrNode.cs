///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.pattern.core;

namespace com.espertech.esper.common.@internal.epl.pattern.or
{
    /// <summary>
    ///     This class represents an 'or' operator in the evaluation tree representing any event expressions.
    /// </summary>
    public class EvalOrNode : EvalNodeBase
    {
        internal readonly EvalOrFactoryNode factoryNode;

        public EvalOrNode(
            PatternAgentInstanceContext context,
            EvalOrFactoryNode factoryNode,
            EvalNode[] childNodes)
            : base(context)
        {
            this.factoryNode = factoryNode;
            ChildNodes = childNodes;
        }

        public EvalOrFactoryNode FactoryNode => factoryNode;

        public EvalNode[] ChildNodes { get; }

        public override EvalStateNode NewState(Evaluator parentNode)
        {
            return new EvalOrStateNode(parentNode, this);
        }
    }
} // end of namespace