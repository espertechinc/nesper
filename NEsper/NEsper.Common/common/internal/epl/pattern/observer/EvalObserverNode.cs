///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.pattern.core;

namespace com.espertech.esper.common.@internal.epl.pattern.observer
{
    /// <summary>
    ///     This class represents an observer expression in the evaluation tree representing an pattern expression.
    /// </summary>
    public class EvalObserverNode : EvalNodeBase
    {
        internal readonly EvalObserverFactoryNode factoryNode;

        public EvalObserverNode(PatternAgentInstanceContext context, EvalObserverFactoryNode factoryNode)
            : base(context)
        {
            this.factoryNode = factoryNode;
        }

        public EvalObserverFactoryNode FactoryNode => factoryNode;

        public override EvalStateNode NewState(Evaluator parentNode)
        {
            return new EvalObserverStateNode(parentNode, this);
        }
    }
} // end of namespace