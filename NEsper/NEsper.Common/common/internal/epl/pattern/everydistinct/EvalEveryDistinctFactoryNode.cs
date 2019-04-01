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
using com.espertech.esper.common.@internal.epl.expression.time.eval;
using com.espertech.esper.common.@internal.epl.pattern.core;

namespace com.espertech.esper.common.@internal.epl.pattern.everydistinct
{
    /// <summary>
    ///     This class represents an 'every-distinct' operator in the evaluation tree representing an event expression.
    /// </summary>
    public class EvalEveryDistinctFactoryNode : EvalFactoryNodeBase
    {
        protected EvalFactoryNode childNode;

        public ExprEvaluator DistinctExpression { get; set; }

        public MatchedEventConvertor Convertor { get; set; }

        public override bool IsFilterChildNonQuitting => true;

        public override bool IsStateful => true;

        public TimePeriodCompute TimePeriodCompute { get; set; }

        public EvalFactoryNode ChildNode {
            get => childNode;
            set => childNode = value;
        }

        public Type[] DistinctTypes { get; set; }

        public override EvalNode MakeEvalNode(PatternAgentInstanceContext agentInstanceContext, EvalNode parentNode)
        {
            var child = EvalNodeUtil.MakeEvalNodeSingleChild(childNode, agentInstanceContext, parentNode);
            return new EvalEveryDistinctNode(this, child, agentInstanceContext);
        }

        public long AbsExpiry(PatternAgentInstanceContext context)
        {
            var current = context.StatementContext.SchedulingService.Time;
            return current + TimePeriodCompute.DeltaAdd(current, null, true, context.AgentInstanceContext);
        }

        public override void Accept(EvalFactoryNodeVisitor visitor)
        {
            visitor.Visit(this);
            childNode.Accept(visitor);
        }
    }
} // end of namespace