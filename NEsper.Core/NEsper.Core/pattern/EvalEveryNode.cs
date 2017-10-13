///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Reflection;

using com.espertech.esper.compat.logging;

namespace com.espertech.esper.pattern
{
    /// <summary>
    /// This class represents an 'every' operator in the evaluation tree representing an event expression.
    /// </summary>
    public class EvalEveryNode : EvalNodeBase
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly EvalEveryFactoryNode _factoryNode;
        private readonly EvalNode _childNode;
    
        public EvalEveryNode(PatternAgentInstanceContext context, EvalEveryFactoryNode factoryNode, EvalNode childNode)
            : base(context)
        {
            _factoryNode = factoryNode;
            _childNode = childNode;
        }

        public EvalEveryFactoryNode FactoryNode
        {
            get { return _factoryNode; }
        }

        public EvalNode ChildNode
        {
            get { return _childNode; }
        }

        public override EvalStateNode NewState(
            Evaluator parentNode,
            EvalStateNodeNumber stateNodeNumber,
            long stateNodeId)
        {
            return new EvalEveryStateNode(parentNode, this);
        }
    }
} // end of namespace
