///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.pattern.core;

namespace com.espertech.esper.common.@internal.epl.pattern.and
{
    /// <summary>
    /// This class represents an 'and' operator in the evaluation tree representing an event expressions.
    /// </summary>
    public class EvalAndNode : EvalNodeBase
    {
        internal readonly EvalAndFactoryNode factoryNode;
        internal readonly EvalNode[] childNodes;

        public EvalAndNode(
            PatternAgentInstanceContext context,
            EvalAndFactoryNode factoryNode,
            EvalNode[] childNodes)
            : base(context)
        {
            this.factoryNode = factoryNode;
            this.childNodes = childNodes;
        }

        public EvalAndFactoryNode FactoryNode => factoryNode;

        public EvalNode[] ChildNodes => childNodes;

        public override EvalStateNode NewState(Evaluator parentNode)
        {
            return new EvalAndStateNode(parentNode, this);
        }
    }
} // end of namespace