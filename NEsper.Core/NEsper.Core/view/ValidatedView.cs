///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.epl.variable;
using com.espertech.esper.events;
using com.espertech.esper.schedule;
using com.espertech.esper.script;

namespace com.espertech.esper.view
{
    /// <summary>
    /// Interface for views that require validation against stream event types.
    /// </summary>
    public interface ValidatedView
    {
        /// <summary>
        /// Validate the view.
        /// </summary>
        /// <param name="engineImportService">The engine import service.</param>
        /// <param name="streamTypeService">supplies the types of streams against which to validate</param>
        /// <param name="timeProvider">for providing current time</param>
        /// <param name="variableService">for access to variables</param>
        /// <param name="tableService"></param>
        /// <param name="scriptingService">The scripting service.</param>
        /// <param name="exprEvaluatorContext">context for expression evaluation</param>
        /// <param name="configSnapshot">The config snapshot.</param>
        /// <param name="schedulingService">The scheduling service.</param>
        /// <param name="engineURI">The engine URI.</param>
        /// <param name="sqlParameters">The SQL parameters.</param>
        /// <param name="eventAdapterService">The event adapter service.</param>
        /// <param name="statementContext"></param>
        /// <throws>ExprValidationException is thrown to indicate an exception in validating the view</throws>
        void Validate(
            EngineImportService engineImportService,
            StreamTypeService streamTypeService,
            TimeProvider timeProvider,
            VariableService variableService,
            TableService tableService,
            ScriptingService scriptingService,
            ExprEvaluatorContext exprEvaluatorContext,
            ConfigurationInformation configSnapshot,
            SchedulingService schedulingService,
            string engineURI,
            IDictionary<int, IList<ExprNode>> sqlParameters,
            EventAdapterService eventAdapterService,
            StatementContext statementContext);
    }
}
