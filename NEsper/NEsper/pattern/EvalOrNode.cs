///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.compat.logging;

namespace com.espertech.esper.pattern
{
    /// <summary>
    /// This class represents an 'or' operator in the evaluation tree representing any event expressions.
    /// </summary>
    public class EvalOrNode : EvalNodeBase
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly EvalOrFactoryNode _factoryNode;
        private readonly EvalNode[] _childNodes;
    
        public EvalOrNode(PatternAgentInstanceContext context, EvalOrFactoryNode factoryNode, EvalNode[] childNodes)
            : base(context)
        {
            _factoryNode = factoryNode;
            _childNodes = childNodes;
        }

        public EvalOrFactoryNode FactoryNode
        {
            get { return _factoryNode; }
        }

        public EvalNode[] ChildNodes
        {
            get { return _childNodes; }
        }

        public override EvalStateNode NewState(Evaluator parentNode, EvalStateNodeNumber stateNodeNumber, long stateNodeId)
        {
            return new EvalOrStateNode(parentNode, this);
        }
    }
} // end of namespace
