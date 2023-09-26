///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.annotation;
using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.client.hook.type;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.historical.common;
using com.espertech.esper.common.@internal.epl.historical.database.connection;
using com.espertech.esper.common.@internal.settings;


namespace com.espertech.esper.common.@internal.epl.historical.database.core
{
    /// <summary>
    /// Implements a poller viewable that uses a polling strategy, a cache and
    /// some input parameters extracted from event streams to perform the polling.
    /// </summary>
    public class HistoricalEventViewableDatabaseFactory : HistoricalEventViewableFactoryBase
    {
        public bool IsEnableLogging { get; private set; }

        public string DatabaseName { get; set; }

        public string[] InputParameters { get; set; }

        public string PreparedStatementText { get; set; }

        public IDictionary<string, DBOutputTypeDesc> OutputTypes { get; set; }

        public SQLColumnTypeConversion ColumnTypeConversionHook { get; set; }

        public SQLOutputRowConversion OutputRowConversionHook { get; set; }

        public ICollection<Attribute> ContextAttributes { get; set; }
   
        public override HistoricalEventViewable Activate(AgentInstanceContext agentInstanceContext)
        {
            var connectionCache = Init(
                agentInstanceContext.DatabaseConfigService,
                agentInstanceContext.ConfigSnapshot);
            var pollExecStrategy = new PollExecStrategyDBQuery(
                this,
                agentInstanceContext,
                connectionCache);
            return new HistoricalEventViewableDatabase(this, pollExecStrategy, agentInstanceContext);
        }

        public PollExecStrategyDBQuery ActivateFireAndForget(
            ExprEvaluatorContext exprEvaluatorContext,
            StatementContextRuntimeServices services)
        {
            SetupHooks(exprEvaluatorContext.Annotations, services.ImportServiceRuntime);
            var connectionCache = Init(services.DatabaseConfigService, services.ConfigSnapshot);
            return new PollExecStrategyDBQuery(this, exprEvaluatorContext, connectionCache);
        }

        private void SetupHooks(
            Attribute[] annotations,
            ImportServiceRuntime importService)
        {
            try {
                ColumnTypeConversionHook = (SQLColumnTypeConversion)ImportUtil.GetAnnotationHook(
                    annotations,
                    HookType.SQLCOL,
                    typeof(SQLColumnTypeConversion),
                    importService);
                OutputRowConversionHook = (SQLOutputRowConversion)ImportUtil.GetAnnotationHook(
                    annotations,
                    HookType.SQLROW,
                    typeof(SQLOutputRowConversion),
                    importService);
            }
            catch (ExprValidationException e) {
                throw new EPException("Failed to obtain annotation-defined sql-related hook: " + e.Message, e);
            }
        }

        public override void Ready(
            StatementContext statementContext,
            ModuleIncidentals moduleIncidentals,
            bool recovery)
        {
            SetupHooks(statementContext.Annotations, statementContext.ImportServiceRuntime);
        }

        private ConnectionCache Init(
            DatabaseConfigServiceRuntime databaseConfigService,
            Configuration configSnapshot)
        {
            IsEnableLogging = configSnapshot.Common.Logging.IsEnableADO;
            try {
                return databaseConfigService.GetConnectionCache(DatabaseName, PreparedStatementText, ContextAttributes);
            }
            catch (DatabaseConfigException e) {
                throw new EPException("Failed to obtain connection cache: " + e.Message, e);
            }
        }
    }
} // end of namespace