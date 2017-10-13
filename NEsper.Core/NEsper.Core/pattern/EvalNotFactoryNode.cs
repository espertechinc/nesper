///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.compat.logging;

namespace com.espertech.esper.pattern
{
    /// <summary>
    /// This class represents an 'not' operator in the evaluation tree representing any event expressions.
    /// </summary>
    public class EvalNotFactoryNode : EvalNodeFactoryBase{
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public EvalNotFactoryNode() {
        }
    
        public override EvalNode MakeEvalNode(PatternAgentInstanceContext agentInstanceContext, EvalNode parentNode)
        {
            EvalNode child = EvalNodeUtil.MakeEvalNodeSingleChild(this.ChildNodes, agentInstanceContext, parentNode);
            return new EvalNotNode(agentInstanceContext, this, child);
        }
    
        public override String ToString()
        {
            return "EvalNotNode children=" + this.ChildNodes.Count;
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
} // end of namespace
