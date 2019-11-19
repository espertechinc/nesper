///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.pattern.core;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.pattern.followedby
{
    /// <summary>
    ///     This class represents a followed-by operator in the evaluation tree representing any event expressions.
    /// </summary>
    public class EvalFollowedByFactoryNode : EvalFactoryNodeBase,
        StatementReadyCallback
    {
        internal EvalFactoryNode[] children;
        private ExprEvaluator[] maxPerChildEvals;
        internal EvalFollowedByNodeOpType opType;

        public EvalFactoryNode[] Children {
            set => children = value;
        }

        public ExprEvaluator[] MaxPerChildEvals {
            set {
                maxPerChildEvals = value;
                if (value != null && value.Length == 0) {
                    throw new IllegalStateException("Invalid empty array");
                }
            }
        }

        public EvalFollowedByNodeOpType OpType => opType;

        public override bool IsFilterChildNonQuitting => false;

        public override bool IsStateful => true;

        public void Ready(
            StatementContext statementContext,
            ModuleIncidentals moduleIncidentals,
            bool recovery)
        {
            var hasMax = maxPerChildEvals != null;
            var hasEngineWidePatternCount =
                statementContext.RuntimeSettingsService.ConfigurationRuntime.Patterns.MaxSubexpressions != null;

            if (!hasMax) {
                opType = hasEngineWidePatternCount
                    ? EvalFollowedByNodeOpType.NOMAX_POOL
                    : EvalFollowedByNodeOpType.NOMAX_PLAIN;
            }
            else {
                opType = hasEngineWidePatternCount
                    ? EvalFollowedByNodeOpType.MAX_POOL
                    : EvalFollowedByNodeOpType.MAX_PLAIN;
            }
        }

        public override EvalNode MakeEvalNode(
            PatternAgentInstanceContext agentInstanceContext,
            EvalNode parentNode)
        {
            var nodes = EvalNodeUtil.MakeEvalNodeChildren(children, agentInstanceContext, parentNode);
            return new EvalFollowedByNode(agentInstanceContext, this, nodes);
        }

        public int GetMax(int position)
        {
            var cachedExpr = maxPerChildEvals[position];
            if (cachedExpr == null) {
                return -1; // no limit defined for this sub-expression
            }

            var result = cachedExpr.Evaluate(null, true, null);
            if (result != null) {
                return result.AsInt();
            }

            return -1; // no limit
        }

        public override void Accept(EvalFactoryNodeVisitor visitor)
        {
            visitor.Visit(this);
            foreach (var child in children) {
                child.Accept(visitor);
            }
        }
    }
} // end of namespace