///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;

namespace com.espertech.esper.pattern
{
    /// <summary>
    /// This class represents a match-until observer in the evaluation tree representing any event expressions.
    /// </summary>
    [Serializable]
    public class EvalMatchUntilFactoryNode : EvalNodeFactoryBase
    {
        [NonSerialized]
        private MatchedEventConvertor _convertor;

        /// <summary>Ctor. </summary>
        public EvalMatchUntilFactoryNode(ExprNode lowerBounds, ExprNode upperBounds, ExprNode singleBound)
        {
            if (singleBound != null && (lowerBounds != null || upperBounds != null)) {
                throw new ArgumentException("Invalid bounds, specify either single bound or range bounds");
            }
            LowerBounds = lowerBounds;
            UpperBounds = upperBounds;
            SingleBound = singleBound;
        }
    
        public override EvalNode MakeEvalNode(PatternAgentInstanceContext agentInstanceContext, EvalNode parentNode)
        {
            EvalNode[] children = EvalNodeUtil.MakeEvalNodeChildren(ChildNodes, agentInstanceContext, parentNode);
            return new EvalMatchUntilNode(agentInstanceContext, this, children[0], children.Length == 1 ? null : children[1]);
        }

        /// <summary>Returns an array of tags for events, which is all tags used within the repeat-operator. </summary>
        /// <value>array of tags</value>
        public int[] TagsArrayed { get; set; }

        public ExprNode LowerBounds { get; set; }

        public ExprNode UpperBounds { get; set; }

        public ExprNode SingleBound { get; set; }

        /// <summary>Sets the convertor for matching events to events-per-stream. </summary>
        /// <value>convertor</value>
        public MatchedEventConvertor Convertor
        {
            get { return _convertor; }
            set { _convertor = value; }
        }

        public override String ToString()
        {
            return ("EvalMatchUntilNode children=" + ChildNodes.Count);
        }

        public override bool IsFilterChildNonQuitting
        {
            get { return true; }
        }

        public override bool IsStateful
        {
            get { return true; }
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer) {
            if (SingleBound != null) {
                writer.Write("[");
                writer.Write(ExprNodeUtility.ToExpressionStringMinPrecedenceSafe(SingleBound));
                writer.Write("] ");
            }
            else {
                if (LowerBounds != null || UpperBounds != null) {
                    writer.Write("[");
                    if (LowerBounds != null) {
                        writer.Write(ExprNodeUtility.ToExpressionStringMinPrecedenceSafe(LowerBounds));
                    }
                    writer.Write(":");
                    if (UpperBounds != null) {
                        writer.Write(ExprNodeUtility.ToExpressionStringMinPrecedenceSafe(UpperBounds));
                    }
                    writer.Write("] ");
                }
            }
            ChildNodes[0].ToEPL(writer, Precedence);
            if (ChildNodes.Count > 1) {
                writer.Write(" until ");
                ChildNodes[1].ToEPL(writer, Precedence);
            }
        }

        public override PatternExpressionPrecedenceEnum Precedence
        {
            get { return PatternExpressionPrecedenceEnum.REPEAT_UNTIL; }
        }
    }
}
