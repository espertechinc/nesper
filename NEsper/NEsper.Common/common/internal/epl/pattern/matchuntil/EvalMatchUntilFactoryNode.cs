///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.pattern.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

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
            EvalNode[] nodes = EvalNodeUtil.MakeEvalNodeChildren(children, agentInstanceContext, parentNode);
            return new EvalMatchUntilNode(agentInstanceContext, this, nodes[0], nodes.Length == 1 ? null : nodes[1]);
        }

        public ExprEvaluator LowerBounds {
            get => lowerBounds;
            set { this.lowerBounds = value; }
        }

        public ExprEvaluator UpperBounds {
            get => upperBounds;
            set { this.upperBounds = value; }
        }

        public ExprEvaluator SingleBound {
            get => singleBound;
            set { this.singleBound = value; }
        }

        public EvalFactoryNode[] Children {
            get => children;
            set { this.children = value; }
        }

        public MatchedEventConvertor OptionalConvertor {
            get => optionalConvertor;
            set { this.optionalConvertor = value; }
        }

        public int[] TagsArrayed {
            get => tagsArrayed;
            set { this.tagsArrayed = value; }
        }

        public override bool IsFilterChildNonQuitting {
            get => true;
        }

        public override bool IsStateful {
            get => true;
        }

        public override void Accept(EvalFactoryNodeVisitor visitor)
        {
            visitor.Visit(this);
            foreach (EvalFactoryNode child in children) {
                child.Accept(visitor);
            }
        }
    }
} // end of namespace