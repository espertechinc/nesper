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
using System.Reflection;

using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.time;

namespace com.espertech.esper.pattern
{
    /// <summary>
    ///     This class represents an 'every-distinct' operator in the evaluation tree representing an event expression.
    /// </summary>
    public class EvalEveryDistinctFactoryNode : EvalNodeFactoryBase
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IList<ExprNode> _expressions;
        [NonSerialized] private MatchedEventConvertor _convertor;
        private IList<ExprNode> _distinctExpressions;
        [NonSerialized] private ExprEvaluator[] _distinctExpressionsArray;
        private ExprNode _expiryTimeExp;
        private ExprTimePeriodEvalDeltaConst _timeDeltaComputation;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="expressions">distinct-value expressions</param>
        protected EvalEveryDistinctFactoryNode(IList<ExprNode> expressions)
        {
            _expressions = expressions;
        }

        public ExprEvaluator[] DistinctExpressionsArray
        {
            get { return _distinctExpressionsArray; }
        }

        /// <summary>
        ///     Gets or sets the convertor for matching events to events-per-stream.
        /// </summary>
        public MatchedEventConvertor Convertor
        {
            get { return _convertor; }
            set { _convertor = value; }
        }

        /// <summary>
        ///     Returns all expressions.
        /// </summary>
        /// <value>expressions</value>
        public IList<ExprNode> Expressions
        {
            get { return _expressions; }
        }

        /// <summary>
        ///     Returns distinct expressions.
        /// </summary>
        /// <value>expressions</value>
        public IList<ExprNode> DistinctExpressions
        {
            get { return _distinctExpressions; }
        }

        public override bool IsFilterChildNonQuitting
        {
            get { return true; }
        }

        public override bool IsStateful
        {
            get { return true; }
        }

        public ExprTimePeriodEvalDeltaConst TimeDeltaComputation
        {
            get { return _timeDeltaComputation; }
        }

        public override PatternExpressionPrecedenceEnum Precedence
        {
            get { return PatternExpressionPrecedenceEnum.UNARY; }
        }

        public override EvalNode MakeEvalNode(PatternAgentInstanceContext agentInstanceContext, EvalNode parentNode)
        {
            if (_distinctExpressionsArray == null)
            {
                _distinctExpressionsArray = ExprNodeUtility.GetEvaluators(_distinctExpressions);
            }
            EvalNode child = EvalNodeUtil.MakeEvalNodeSingleChild(ChildNodes, agentInstanceContext, parentNode);
            return new EvalEveryDistinctNode(this, child, agentInstanceContext);
        }

        public override String ToString()
        {
            return "EvalEveryNode children=" + ChildNodes.Count;
        }

        public void SetDistinctExpressions(
            IList<ExprNode> distinctExpressions,
            ExprTimePeriodEvalDeltaConst timeDeltaComputation,
            ExprNode expiryTimeExp)
        {
            _distinctExpressions = distinctExpressions;
            _timeDeltaComputation = timeDeltaComputation;
            _expiryTimeExp = expiryTimeExp;
        }

        public long AbsExpiry(PatternAgentInstanceContext context)
        {
            long current = context.StatementContext.SchedulingService.Time;
            return current + _timeDeltaComputation.DeltaAdd(current);
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            writer.Write("every-Distinct(");
            ExprNodeUtility.ToExpressionStringParameterList(_distinctExpressions, writer);
            if (_expiryTimeExp != null)
            {
                writer.Write(",");
                writer.Write(ExprNodeUtility.ToExpressionStringMinPrecedenceSafe(_expiryTimeExp));
            }
            writer.Write(") ");
            ChildNodes[0].ToEPL(writer, Precedence);
        }
    }
} // end of namespace