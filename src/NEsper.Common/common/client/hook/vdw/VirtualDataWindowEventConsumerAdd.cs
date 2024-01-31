///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.client.hook.vdw
{
    /// <summary>
    ///     Event indicating a named-window consuming statement is being added.
    /// </summary>
    public class VirtualDataWindowEventConsumerAdd : VirtualDataWindowEventConsumerBase
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="namedWindowName">the named window name</param>
        /// <param name="consumerObject">
        ///     an object that identifies the consumer, the same instance or the add and for the remove
        ///     event
        /// </param>
        /// <param name="statementName">statement name</param>
        /// <param name="agentInstanceId">agent instance id</param>
        /// <param name="filter">filter expressions</param>
        /// <param name="exprEvaluatorContext">for expression evaluation</param>
        public VirtualDataWindowEventConsumerAdd(
            string namedWindowName,
            object consumerObject,
            string statementName,
            int agentInstanceId,
            ExprEvaluator filter,
            ExprEvaluatorContext exprEvaluatorContext)
            : base(
                namedWindowName,
                consumerObject,
                statementName,
                agentInstanceId)
        {
            Filter = filter;
            ExprEvaluatorContext = exprEvaluatorContext;
        }

        /// <summary>
        ///     Provides the filter expressions.
        ///     <para>
        ///     Evaluate filter expressions, if any, as follows:
        ///     <code>
        ///     bool pass = filter[...].getExprEvaluator().evaluate(
        ///         new EventBean[] {vdwEvent}, true,
        ///         addEvent.getExprEvaluatorContext());
        ///     </code>
        /// </para>
        ///     <para>
        ///     Filter expressions must be evaluated using the same ExprEvaluatorContext instance as provided by this event.
        /// </para>
        /// </summary>
        /// <returns>filter expression list</returns>
        public ExprEvaluator Filter { get; }

        /// <summary>
        ///     Returns the expression evaluator context for evaluating filter expressions.
        /// </summary>
        /// <returns>expression evaluator context</returns>
        public ExprEvaluatorContext ExprEvaluatorContext { get; }
    }
} // end of namespace