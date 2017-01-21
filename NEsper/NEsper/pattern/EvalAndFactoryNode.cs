///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

namespace com.espertech.esper.pattern
{
    /// <summary>
    /// This class represents an 'and' operator in the evaluation tree representing an event expressions.
    /// </summary>
    public class EvalAndFactoryNode : EvalNodeFactoryBase
    {
        public EvalAndFactoryNode()
        {
        }
    
        public override EvalNode MakeEvalNode(PatternAgentInstanceContext agentInstanceContext, EvalNode parentNode)
        {
            EvalNode[] children = EvalNodeUtil.MakeEvalNodeChildren(ChildNodes, agentInstanceContext, parentNode);
            return new EvalAndNode(agentInstanceContext, this, children);
        }
    
        public override String ToString()
        {
            return ("EvalAndFactoryNode children=" + ChildNodes.Count);
        }

        public override bool IsFilterChildNonQuitting
        {
            get { return false; }
        }

        public override bool IsStateful
        {
            get { return true; }
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            PatternExpressionUtil.ToPrecedenceFreeEPL(writer, "and", ChildNodes, Precedence);
        }

        public override PatternExpressionPrecedenceEnum Precedence
        {
            get { return PatternExpressionPrecedenceEnum.AND; }
        }
    }
}
