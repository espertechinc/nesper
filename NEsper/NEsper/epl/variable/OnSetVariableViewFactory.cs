///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Reflection;

using com.espertech.esper.client;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.spec;
using com.espertech.esper.events;

namespace com.espertech.esper.epl.variable
{
    /// <summary>
    ///     A factory for a view that handles the setting of variables upon receipt of a triggering event.
    /// </summary>
    public class OnSetVariableViewFactory
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="desc">specification for the on-set statement</param>
        /// <param name="eventAdapterService">for creating statements</param>
        /// <param name="variableService">for setting variables</param>
        /// <param name="statementResultService">for coordinating on whether insert and remove stream events should be posted</param>
        /// <param name="exprEvaluatorContext">context for expression evalauation</param>
        /// <param name="statementId">statement id</param>
        /// <exception cref="com.espertech.esper.epl.expression.core.ExprValidationException">
        ///     if the assignment expressions are
        ///     invalid
        /// </exception>
        public OnSetVariableViewFactory(
            int statementId,
            OnTriggerSetDesc desc,
            EventAdapterService eventAdapterService,
            VariableService variableService,
            StatementResultService statementResultService,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            EventAdapterService = eventAdapterService;
            VariableService = variableService;
            StatementResultService = statementResultService;

            VariableReadWritePackage = new VariableReadWritePackage(
                desc.Assignments, variableService, eventAdapterService);
            string outputEventTypeName = statementId + "_outsetvar";
            EventType = eventAdapterService.CreateAnonymousMapType(
                outputEventTypeName, VariableReadWritePackage.VariableTypes, true);
        }

        public EventType EventType { get; private set; }

        public EventAdapterService EventAdapterService { get; private set; }

        public VariableService VariableService { get; private set; }

        public VariableReadWritePackage VariableReadWritePackage { get; private set; }

        public StatementResultService StatementResultService { get; private set; }

        public OnSetVariableView Instantiate(ExprEvaluatorContext exprEvaluatorContext)
        {
            return new OnSetVariableView(this, exprEvaluatorContext);
        }
    }
} // end of namespace