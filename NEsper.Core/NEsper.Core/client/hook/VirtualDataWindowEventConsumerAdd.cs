///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.client.hook
{
    /// <summary>
    /// Event indicating a named-window consuming statement is being added.
    /// </summary>
    public class VirtualDataWindowEventConsumerAdd : VirtualDataWindowEventConsumerBase
    {
        /// <summary>Ctor. </summary>
        /// <param name="namedWindowName">the named window name</param>
        /// <param name="consumerObject">an object that identifies the consumer, the same instance or the add and for the remove event</param>
        /// <param name="statementName">statement name</param>
        /// <param name="agentInstanceId">agent instance id</param>
        /// <param name="filterExpressions">filter expressions</param>
        /// <param name="exprEvaluatorContext">for expression evaluation</param>
        public VirtualDataWindowEventConsumerAdd(
            String namedWindowName,
            Object consumerObject,
            String statementName,
            int agentInstanceId,
            ExprNode[] filterExpressions,
            ExprEvaluatorContext exprEvaluatorContext)
            : base(namedWindowName, consumerObject, statementName, agentInstanceId)
        {
            FilterExpressions = filterExpressions;
            ExprEvaluatorContext = exprEvaluatorContext;
        }

        /// <summary>Provides the filter expressions. <para />Evaluate filter expressions, if any, as follows: Boolean pass = filterExpressions[...].ExprEvaluator.Evaluate(new EventBean[] {vdwEvent}, true, addEvent._exprEvaluatorContext); <para />Filter expressions must be evaluated using the same _exprEvaluatorContext instance as provided by this event. </summary>
        /// <value>filter expression list</value>
        public ExprNode[] FilterExpressions { get; private set; }

        /// <summary>Returns the expression evaluator context for evaluating filter expressions. </summary>
        /// <value>expression evaluator context</value>
        public ExprEvaluatorContext ExprEvaluatorContext { get; private set; }
    }
}