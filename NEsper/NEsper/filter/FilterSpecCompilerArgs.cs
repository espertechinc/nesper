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
using com.espertech.esper.collection;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.container;
using com.espertech.esper.compat.threading;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.epl.variable;
using com.espertech.esper.events;
using com.espertech.esper.schedule;
using com.espertech.esper.script;

namespace com.espertech.esper.filter
{
    public class FilterSpecCompilerArgs
    {
        public readonly Attribute[] Annotations;
        public readonly IDictionary<string, Pair<EventType, string>> ArrayEventTypes;
        public readonly ConfigurationInformation ConfigurationInformation;
        public readonly ContextDescriptor ContextDescriptor;
        public readonly EventAdapterService EventAdapterService;
        public readonly FilterBooleanExpressionFactory FilterBooleanExpressionFactory;
        public readonly ExprEvaluatorContext ExprEvaluatorContext;
        public readonly EngineImportService EngineImportService;
        public readonly ScriptingService ScriptingService;
        public readonly int StatementId;
        public readonly string StatementName;
        public readonly StreamTypeService StreamTypeService;
        public readonly TableService TableService;
        public readonly IDictionary<string, Pair<EventType, string>> TaggedEventTypes;
        public readonly TimeProvider TimeProvider;
        public readonly VariableService VariableService;
        public readonly StatementExtensionSvcContext StatementExtensionSvcContext;
        public readonly IContainer Container;

        public FilterSpecCompilerArgs(
            IContainer container,
            IDictionary<string, Pair<EventType, string>> taggedEventTypes,
            IDictionary<string, Pair<EventType, string>> arrayEventTypes,
            ExprEvaluatorContext exprEvaluatorContext,
            string statementName,
            int statementId,
            StreamTypeService streamTypeService,
            EngineImportService engineImportService,
            TimeProvider timeProvider,
            VariableService variableService,
            TableService tableService,
            EventAdapterService eventAdapterService,
            FilterBooleanExpressionFactory filterBooleanExpressionFactory,
            ScriptingService scriptingService,
            Attribute[] annotations,
            ContextDescriptor contextDescriptor,
            ConfigurationInformation configurationInformation,
            StatementExtensionSvcContext statementExtensionSvcContext)
        {
            Container = container;
            TaggedEventTypes = taggedEventTypes;
            ArrayEventTypes = arrayEventTypes;
            ExprEvaluatorContext = exprEvaluatorContext;
            StatementName = statementName;
            StatementId = statementId;
            StreamTypeService = streamTypeService;
            EngineImportService = engineImportService;
            TimeProvider = timeProvider;
            VariableService = variableService;
            TableService = tableService;
            EventAdapterService = eventAdapterService;
            FilterBooleanExpressionFactory = filterBooleanExpressionFactory;
            ScriptingService = scriptingService;
            Annotations = annotations;
            ContextDescriptor = contextDescriptor;
            ConfigurationInformation = configurationInformation;
            StatementExtensionSvcContext = statementExtensionSvcContext;
        }
    }
} // end of namespace