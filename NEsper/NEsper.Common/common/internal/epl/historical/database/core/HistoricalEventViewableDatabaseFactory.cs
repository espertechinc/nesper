///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.annotation;
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
    ///     Implements a poller viewable that uses a polling strategy, a cache and
    ///     some input parameters extracted from event streams to perform the polling.
    /// </summary>
    public class HistoricalEventViewableDatabaseFactory : HistoricalEventViewableFactoryBase
    {
        internal SQLColumnTypeConversion columnTypeConversionHook;

        internal string databaseName;
        internal bool enableLogging;
        internal string[] inputParameters;
        internal SQLOutputRowConversion outputRowConversionHook;
        internal IDictionary<string, DBOutputTypeDesc> outputTypes;
        internal string preparedStatementText;

        public string DatabaseName {
            get => databaseName;
            set => databaseName = value;
        }

        public string[] InputParameters {
            get => inputParameters;
            set => inputParameters = value;
        }

        public string PreparedStatementText {
            get => preparedStatementText;
            set => preparedStatementText = value;
        }

        public IDictionary<string, DBOutputTypeDesc> OutputTypes {
            get => outputTypes;
            set => outputTypes = value;
        }

        public SQLColumnTypeConversion ColumnTypeConversionHook {
            get => columnTypeConversionHook;
            set => columnTypeConversionHook = value;
        }

        public SQLOutputRowConversion OutputRowConversionHook {
            get => outputRowConversionHook;
            set => outputRowConversionHook = value;
        }

        public override HistoricalEventViewable Activate(AgentInstanceContext agentInstanceContext)
        {
            ConnectionCache connectionCache = null;
            try {
                connectionCache =
                    agentInstanceContext.DatabaseConfigService.GetConnectionCache(databaseName, preparedStatementText, TODO);
            }
            catch (DatabaseConfigException e) {
                throw new EPException("Failed to obtain connection cache: " + e.Message, e);
            }

            var pollExecStrategy = new PollExecStrategyDBQuery(this, agentInstanceContext, connectionCache);
            return new HistoricalEventViewableDatabase(this, pollExecStrategy, agentInstanceContext);
        }

        public override void Ready(StatementContext statementContext, ModuleIncidentals moduleIncidentals, bool recovery)
        {
            try {
                columnTypeConversionHook = (SQLColumnTypeConversion) ImportUtil
                    .GetAnnotationHook(
                        statementContext.Annotations, HookType.SQLCOL, typeof(SQLColumnTypeConversion),
                        statementContext.ImportServiceRuntime);
                outputRowConversionHook = (SQLOutputRowConversion) ImportUtil
                    .GetAnnotationHook(
                        statementContext.Annotations, HookType.SQLROW, typeof(SQLOutputRowConversion),
                        statementContext.ImportServiceRuntime);
            }
            catch (ExprValidationException e) {
                throw new EPException("Failed to obtain annotation-defined sql-related hook: " + e.Message, e);
            }
        }
    }
} // end of namespace