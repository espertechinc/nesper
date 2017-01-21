///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.compat.threading;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.script;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.schedule;

namespace com.espertech.esper.epl.expression.core
{
    /// <summary>
    /// Returns the context for expression evaluation.
    /// </summary>
    public interface ExprEvaluatorContext
    {
        string StatementName { get; }

        object StatementUserObject { get; }

        string EngineURI { get; }

        int StatementId { get; }

        StatementType? StatementType { get; }

        TimeProvider TimeProvider { get; }

        ExpressionResultCacheService ExpressionResultCacheService { get; }

        int AgentInstanceId { get; }

        EventBean ContextProperties { get; }

        AgentInstanceScriptContext AgentInstanceScriptContext { get; }

        IReaderWriterLock AgentInstanceLock { get; }

        TableExprEvaluatorContext TableExprEvaluatorContext { get; }
    }

    public class ProxyExprEvaluatorContext : ExprEvaluatorContext
    {
        public Func<object> ProcStatementUserObject { get; set; } 
        public Func<TimeProvider> ProcTimeProvider { get; set; }
        public Func<ExpressionResultCacheService> ProcExpressionResultCacheService { get; set; }
        public Func<int> ProcAgentInstanceId { get; set; }
        public Func<EventBean> ProcContextProperties { get; set; }
        public Func<string> ProcStatementName { get; set; }
        public Func<string> ProcEngineURI { get; set; }
        public Func<int> ProcStatementId { get; set; }
        public Func<StatementType?> ProcStatementType { get; set; }
        public Func<AgentInstanceScriptContext> ProcAgentInstanceScriptContext { get; set; }
        public Func<IReaderWriterLock> ProcAgentInstanceLock { get; set; }
        public Func<TableExprEvaluatorContext> ProcTableExprEvaluatorContext { get; set; }

        public ProxyExprEvaluatorContext()
        {
            ProcTimeProvider = () => null;
            ProcExpressionResultCacheService = () => null;
            ProcAgentInstanceId = () => -1;
            ProcContextProperties = () => null;
            ProcStatementName = () => null;
            ProcEngineURI = () => null;
            ProcStatementId = () => -1;
            ProcStatementType = () => null;
            ProcAgentInstanceScriptContext = () => null;
            ProcAgentInstanceLock = () => null;
            ProcTableExprEvaluatorContext = () => null;
        }

        public object StatementUserObject
        {
            get { return ProcStatementUserObject(); }
        }

        public TimeProvider TimeProvider
        {
            get { return ProcTimeProvider(); }
        }

        public ExpressionResultCacheService ExpressionResultCacheService
        {
            get { return ProcExpressionResultCacheService(); }
        }

        public int AgentInstanceId
        {
            get { return ProcAgentInstanceId(); }
        }

        public EventBean ContextProperties
        {
            get { return ProcContextProperties(); }
        }

        public string StatementName
        {
            get { return ProcStatementName(); }
        }

        public string EngineURI
        {
            get { return ProcEngineURI(); }
        }

        public int StatementId
        {
            get { return ProcStatementId(); }
        }

        public StatementType? StatementType
        {
            get { return ProcStatementType(); }
        }

        public AgentInstanceScriptContext AgentInstanceScriptContext
        {
            get { return ProcAgentInstanceScriptContext(); }
        }

        public IReaderWriterLock AgentInstanceLock
        {
            get { return ProcAgentInstanceLock(); }
        }

        public TableExprEvaluatorContext TableExprEvaluatorContext
        {
            get { return ProcTableExprEvaluatorContext(); }
        }

    }
}
