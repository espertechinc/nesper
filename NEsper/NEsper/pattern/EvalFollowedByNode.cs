///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.pattern
{
    /// <summary>
    /// This class represents a followed-by operator in the evaluation tree representing any event expressions.
    /// </summary>
    [Serializable]
    public class EvalFollowedByNode : EvalNodeBase
    {
        private readonly EvalFollowedByFactoryNode _factoryNode;
        private readonly EvalNode[] _childNodes;

        public EvalFollowedByNode(PatternAgentInstanceContext context, EvalFollowedByFactoryNode factoryNode, EvalNode[] childNodes)
            : base(context)
        {
            _factoryNode = factoryNode;
            _childNodes = childNodes;
        }

        public EvalNode[] ChildNodes
        {
            get { return _childNodes; }
        }

        public EvalFollowedByFactoryNode FactoryNode
        {
            get { return _factoryNode; }
        }

        public override EvalStateNode NewState(Evaluator parentNode, EvalStateNodeNumber stateNodeNumber, long stateNodeId)
        {
            switch (_factoryNode.OpType)
            {
                case EvalFollowedByNodeOpType.NOMAX_PLAIN:
                    return new EvalFollowedByStateNode(parentNode, this);
                default:
                    return new EvalFollowedByWithMaxStateNodeManaged(parentNode, this);
            }
        }

        public bool IsTrackWithPool
        {
            get
            {
                return _factoryNode.OpType == EvalFollowedByNodeOpType.NOMAX_POOL ||
                       _factoryNode.OpType == EvalFollowedByNodeOpType.MAX_POOL;
            }
        }

        public bool IsTrackWithMax
        {
            get
            {
                return _factoryNode.OpType == EvalFollowedByNodeOpType.MAX_PLAIN ||
                       _factoryNode.OpType == EvalFollowedByNodeOpType.MAX_POOL;
            }
        }
    }
}
