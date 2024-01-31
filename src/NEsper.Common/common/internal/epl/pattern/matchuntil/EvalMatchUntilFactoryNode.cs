///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.pattern.core;

namespace com.espertech.esper.common.@internal.epl.pattern.matchuntil
{
    /// <summary>
    /// This class represents a match-until observer in the evaluation tree representing any event expressions.
    /// </summary>
    public class EvalMatchUntilFactoryNode : EvalFactoryNodeBase
    {
        private ExprEvaluator lowerBounds;
        private ExprEvaluator upperBounds;
        private ExprEvaluator singleBound;
        private int[] tagsArrayed;
        protected EvalFactoryNode[] children;
        private MatchedEventConvertor optionalConvertor;

        public override EvalNode MakeEvalNode(
            PatternAgentInstanceContext agentInstanceContext,
            EvalNode parentNode)
        {
            var nodes = EvalNodeUtil.MakeEvalNodeChildren(children, agentInstanceContext, parentNode);
            return new EvalMatchUntilNode(agentInstanceContext, this, nodes[0], nodes.Length == 1 ? null : nodes[1]);
        }

        public ExprEvaluator LowerBounds {
            get => lowerBounds;
            set => lowerBounds = value;
        }

        public ExprEvaluator UpperBounds {
            get => upperBounds;
            set => upperBounds = value;
        }

        public ExprEvaluator SingleBound {
            get => singleBound;
            set => singleBound = value;
        }

        public EvalFactoryNode[] Children {
            get => children;
            set => children = value;
        }

        public MatchedEventConvertor OptionalConvertor {
            get => optionalConvertor;
            set => optionalConvertor = value;
        }

        public int[] TagsArrayed {
            get => tagsArrayed;
            set => tagsArrayed = value;
        }

        public override bool IsFilterChildNonQuitting => true;

        public override bool IsStateful => true;

        public override void Accept(EvalFactoryNodeVisitor visitor)
        {
            visitor.Visit(this);
            foreach (var child in children) {
                child.Accept(visitor);
            }
        }
    }
} // end of namespace