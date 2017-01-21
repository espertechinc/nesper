///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;

using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.time;

namespace com.espertech.esper.pattern
{
    /// <summary>
    /// This class represents an 'every-distinct' operator in the evaluation tree representing an event expression.
    /// </summary>
    [Serializable]
    public class EvalEveryDistinctFactoryNode : EvalNodeFactoryBase
    {
        [NonSerialized] protected ExprEvaluator[] distinctExpressionsArray;
        [NonSerialized] private MatchedEventConvertor convertor;
        private ExprNode _expiryTimeExp;

        /// <summary>Ctor. </summary>
        /// <param name="expressions">distinct-value expressions</param>
        public EvalEveryDistinctFactoryNode(IList<ExprNode> expressions)
        {
            Expressions = expressions;
        }
    
        public override EvalNode MakeEvalNode(PatternAgentInstanceContext agentInstanceContext, EvalNode parentNode) {
            if (distinctExpressionsArray == null) {
                distinctExpressionsArray = ExprNodeUtility.GetEvaluators(DistinctExpressions);
            }
            EvalNode child = EvalNodeUtil.MakeEvalNodeSingleChild(ChildNodes, agentInstanceContext, parentNode);
            return new EvalEveryDistinctNode(this, child, agentInstanceContext);
        }

        public ExprEvaluator[] DistinctExpressionsArray
        {
            get { return distinctExpressionsArray; }
        }

        /// <summary>Sets the convertor for matching events to events-per-stream. </summary>
        /// <value>convertor</value>
        public MatchedEventConvertor Convertor
        {
            get { return convertor; }
            set { convertor = value; }
        }

        public ExprTimePeriodEvalDeltaConst TimeDeltaComputation { get; private set; }

        public override String ToString()
        {
            return "EvalEveryNode children=" + ChildNodes.Count;
        }

        /// <summary>Returns all expressions. </summary>
        /// <value>expressions</value>
        public IList<ExprNode> Expressions { get; protected set; }

        /// <summary>Returns distinct expressions. </summary>
        /// <value>expressions</value>
        public IList<ExprNode> DistinctExpressions { get; protected set; }

        /// <summary>
        /// Sets expressions for distinct-value.
        /// </summary>
        /// <param name="distinctExpressions">to set</param>
        /// <param name="expiryTimeExp">The expiry time exp.</param>
        public void SetDistinctExpressions(IList<ExprNode> distinctExpressions, ExprTimePeriodEvalDeltaConst timeDeltaComputation, ExprNode expiryTimeExp)
        {
            DistinctExpressions = distinctExpressions;
            TimeDeltaComputation = timeDeltaComputation;
            _expiryTimeExp = expiryTimeExp;
        }

        public override bool IsFilterChildNonQuitting
        {
            get { return true; }
        }

        public override bool IsStateful
        {
            get { return true; }
        }

        public long AbsMillisecondExpiry(PatternAgentInstanceContext context)
        {
            long current = context.StatementContext.SchedulingService.Time;
            return current + TimeDeltaComputation.DeltaMillisecondsAdd(current);
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            writer.Write("every-distinct(");
            ExprNodeUtility.ToExpressionStringParameterList(DistinctExpressions, writer);
            if (_expiryTimeExp != null) {
                writer.Write(",");
                writer.Write(ExprNodeUtility.ToExpressionStringMinPrecedenceSafe(_expiryTimeExp));
            }
            writer.Write(") ");
            ChildNodes[0].ToEPL(writer, Precedence);
        }

        public override PatternExpressionPrecedenceEnum Precedence
        {
            get { return PatternExpressionPrecedenceEnum.UNARY; }
        }
    }
}
