///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
    /// This class represents an 'not' operator in the evaluation tree representing any event expressions.
    /// </summary>
    public class EvalNotFactoryNode : EvalNodeFactoryBase
    {
        public EvalNotFactoryNode()
        {
        }

        public override EvalNode MakeEvalNode(PatternAgentInstanceContext agentInstanceContext, EvalNode parentNode)
        {
            EvalNode child = EvalNodeUtil.MakeEvalNodeSingleChild(this.ChildNodes, agentInstanceContext, parentNode);
            return new EvalNotNode(agentInstanceContext, this, child);
        }

        public override String ToString()
        {
            return "EvalNotNode children=" + ChildNodes.Count;
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
            writer.Write("not ");
            ChildNodes[0].ToEPL(writer, Precedence);
        }

        public override PatternExpressionPrecedenceEnum Precedence
        {
            get { return PatternExpressionPrecedenceEnum.UNARY; }
        }
    }
}