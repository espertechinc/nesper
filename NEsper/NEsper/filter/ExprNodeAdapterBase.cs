///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

using com.espertech.esper.client;
using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;

namespace com.espertech.esper.filter
{
    public class ExprNodeAdapterBase
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly int _filterSpecId;
        private readonly int _filterSpecParamPathNum;
        private readonly ExprNode _exprNode;
        private readonly ExprEvaluator _exprNodeEval;
        private readonly ExprEvaluatorContext _evaluatorContext;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="filterSpecId">The filter spec identifier.</param>
        /// <param name="filterSpecParamPathNum">The filter spec parameter path number.</param>
        /// <param name="exprNode">is the bool expression</param>
        /// <param name="evaluatorContext">The evaluator context.</param>
        public ExprNodeAdapterBase(int filterSpecId, int filterSpecParamPathNum, ExprNode exprNode, ExprEvaluatorContext evaluatorContext)
        {
            _filterSpecId = filterSpecId;
            _filterSpecParamPathNum = filterSpecParamPathNum;
            _exprNode = exprNode;
            _exprNodeEval = exprNode.ExprEvaluator;
            _evaluatorContext = evaluatorContext;
        }
    
        /// <summary>Evaluate the bool expression given the event as a stream zero event. </summary>
        /// <param name="theEvent">is the stream zero event (current event)</param>
        /// <returns>bool result of the expression</returns>
        public virtual bool Evaluate(EventBean theEvent)
        {
            return EvaluatePerStream(new EventBean[] {theEvent});
        }
    
        protected virtual bool EvaluatePerStream(EventBean[] eventsPerStream)
        {
            try
            {
                var result = (bool?) _exprNodeEval.Evaluate(new EvaluateParams(eventsPerStream, true, _evaluatorContext));
                if (result == null)
                {
                    return false;
                }
                return result.Value;
            }
            catch (Exception ex)
            {
                Log.Error("Error evaluating expression '" + ExprNodeUtility.ToExpressionStringMinPrecedenceSafe(_exprNode) + "' statement '" + StatementName + "': " + ex.Message, ex);
                return false;
            }
        }

        public string StatementName
        {
            get { return _evaluatorContext.StatementName; ; }
        }

        public string StatementId
        {
            get { return _evaluatorContext.StatementId; }
        }

        public ExprNode ExprNode
        {
            get { return _exprNode; }
        }

        public int FilterSpecId
        {
            get { return _filterSpecId; }
        }

        public int FilterSpecParamPathNum
        {
            get { return _filterSpecParamPathNum; }
        }

        public ExprEvaluator ExprNodeEval
        {
            get { return _exprNodeEval; }
        }

        public ExprEvaluatorContext EvaluatorContext
        {
            get { return _evaluatorContext; }
        }
    }
}
