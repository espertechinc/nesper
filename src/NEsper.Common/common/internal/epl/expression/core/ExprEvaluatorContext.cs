///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.hook.expr;
using com.espertech.esper.common.@internal.epl.enummethod.cache;
using com.espertech.esper.common.@internal.epl.expression.time.abacus;
using com.espertech.esper.common.@internal.epl.script.core;
using com.espertech.esper.common.@internal.epl.table.core;
using com.espertech.esper.common.@internal.epl.variable.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.metrics.audit;
using com.espertech.esper.common.@internal.metrics.instrumentation;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.compat;
using com.espertech.esper.compat.threading.locks;

namespace com.espertech.esper.common.@internal.epl.expression.core
{
    /// <summary>
    /// Returns the context for expression evaluation.
    /// </summary>
    public interface ExprEvaluatorContext
    {
        string StatementName { get; }

        string ContextName { get; }

        object UserObjectCompileTime { get; }

        string RuntimeURI { get; }

        int StatementId { get; }

        EventBean ContextProperties { get; }

        int AgentInstanceId { get; }

        EventBeanService EventBeanService { get; }

        TimeProvider TimeProvider { get; }

        IReaderWriterLock AgentInstanceLock { get; }

        ExpressionResultCacheService ExpressionResultCacheService { get; }

        TableExprEvaluatorContext TableExprEvaluatorContext { get; }

        AgentInstanceScriptContext AllocateAgentInstanceScriptContext { get; }

        string DeploymentId { get; }

        AuditProvider AuditProvider { get; }

        InstrumentationCommon InstrumentationProvider { get; }

        ExceptionHandlingService ExceptionHandlingService { get; }

        TypeResolver TypeResolver { get; }

        object FilterReboolConstant { get; set; }

        string EPLWhenAvailable { get; }

        TimeZoneInfo TimeZone { get; }

        TimeAbacus TimeAbacus { get; }

        VariableManagementService VariableManagementService { get; }

        EventBeanTypedEventFactory EventBeanTypedEventFactory { get; }

        string ModuleName { get; }

        bool IsWritesToTables { get; }

        Attribute[] Annotations { get; }
    }
} // end of namespace