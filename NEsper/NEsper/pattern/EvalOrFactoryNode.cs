///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Linq;

namespace com.espertech.esper.pattern
{
    /// <summary>
    /// This class represents an 'or' operator in the evaluation tree representing any event expressions.
    /// </summary>
    public class EvalOrFactoryNode : EvalNodeFactoryBase
    {
        public EvalOrFactoryNode()
        {
        }
    
        public override EvalNode MakeEvalNode(PatternAgentInstanceContext agentInstanceContext, EvalNode parentNode)
        {
            EvalNode[] children = EvalNodeUtil.MakeEvalNodeChildren(ChildNodes, agentInstanceContext, parentNode);
            return new EvalOrNode(agentInstanceContext, this, children);
        }
    
        public override String ToString()
        {
            return ("EvalOrNode children=" + ChildNodes.Count);
        }

        public override bool IsFilterChildNonQuitting
        {
            get { return false; }
        }

        public override bool IsStateful
        {
            get { return ChildNodes.Any(child => child.IsStateful); }
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            PatternExpressionUtil.ToPrecedenceFreeEPL(writer, "or", ChildNodes, Precedence);
        }

        public override PatternExpressionPrecedenceEnum Precedence
        {
            get { return PatternExpressionPrecedenceEnum.OR; }
        }
    }
}
