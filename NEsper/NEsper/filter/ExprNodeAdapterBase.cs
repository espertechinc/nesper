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
    
        private readonly String _statementName;
        private readonly ExprNode _exprNode;
        private readonly ExprEvaluator _exprNodeEval;
        private readonly ExprEvaluatorContext _evaluatorContext;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="statementName">Name of the statement.</param>
        /// <param name="exprNode">is the bool expression</param>
        /// <param name="evaluatorContext">The evaluator context.</param>
        public ExprNodeAdapterBase(String statementName, ExprNode exprNode, ExprEvaluatorContext evaluatorContext)
        {
            _statementName = statementName;
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
                Log.Error("Error evaluating expression '" + ExprNodeUtility.ToExpressionStringMinPrecedenceSafe(_exprNode) + "' statement '" + _statementName + "': " + ex.Message, ex);
                return false;
            }
        }

        public string StatementName
        {
            get { return _statementName; }
        }

        public ExprNode ExprNode
        {
            get { return _exprNode; }
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
