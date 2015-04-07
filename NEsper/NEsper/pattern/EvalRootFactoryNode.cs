///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.compat.collections;

namespace com.espertech.esper.pattern
{
    /// <summary>
    /// This class is always the root node in the evaluation tree representing an event 
    /// expression. It hold the handle to the EPStatement implementation for notifying 
    /// when matches are found.
    /// </summary>
    public class EvalRootFactoryNode : EvalNodeFactoryBase
    {
        public EvalRootFactoryNode()
        {
        }

        public override EvalNode MakeEvalNode(PatternAgentInstanceContext agentInstanceContext)
        {
            return MakeEvalNodeRoot(agentInstanceContext);
        }

        public EvalRootNode MakeEvalNodeRoot(PatternAgentInstanceContext agentInstanceContext)
        {
            EvalNode child = EvalNodeUtil.MakeEvalNodeSingleChild(ChildNodes, agentInstanceContext);
            return new EvalRootNode(agentInstanceContext, this, child);
        }

        public override String ToString()
        {
            return ("EvalRootNode children=" + ChildNodes.Count);
        }

        public override bool IsFilterChildNonQuitting
        {
            get { return false; }
        }

        public override bool IsStateful
        {
            get { return ChildNodes[0].IsStateful; }
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            if (!ChildNodes.IsEmpty())
            {
                ChildNodes[0].ToEPL(writer, Precedence);
            }
        }

        public override PatternExpressionPrecedenceEnum Precedence
        {
            get { return PatternExpressionPrecedenceEnum.MINIMUM; }
        }
    }
}
