///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.pattern.core;

namespace com.espertech.esper.common.@internal.epl.pattern.followedby
{
    /// <summary>
    ///     This class represents a followed-by operator in the evaluation tree representing any event expressions.
    /// </summary>
    public class EvalFollowedByNode : EvalNodeBase
    {
        internal readonly EvalFollowedByFactoryNode factoryNode;

        public EvalFollowedByNode(
            PatternAgentInstanceContext context,
            EvalFollowedByFactoryNode factoryNode,
            EvalNode[] childNodes)
            : base(context)
        {
            this.factoryNode = factoryNode;
            ChildNodes = childNodes;
        }

        public EvalNode[] ChildNodes { get; }

        public EvalFollowedByFactoryNode FactoryNode => factoryNode;

        public bool IsTrackWithPool => factoryNode.OpType == EvalFollowedByNodeOpType.NOMAX_POOL ||
                                       factoryNode.OpType == EvalFollowedByNodeOpType.MAX_POOL;

        public bool IsTrackWithMax => factoryNode.OpType == EvalFollowedByNodeOpType.MAX_PLAIN ||
                                      factoryNode.OpType == EvalFollowedByNodeOpType.MAX_POOL;

        public override EvalStateNode NewState(Evaluator parentNode)
        {
            switch (factoryNode.opType) {
                case EvalFollowedByNodeOpType.NOMAX_PLAIN:
                    return new EvalFollowedByStateNode(parentNode, this);
                default:
                    return new EvalFollowedByWithMaxStateNodeManaged(parentNode, this);
            }
        }
    }
} // end of namespace