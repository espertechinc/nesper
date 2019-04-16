///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Reflection;
using com.espertech.esper.collection;
using com.espertech.esper.common.client;
using com.espertech.esper.common.client.hook.type;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.db;
using com.espertech.esper.common.@internal.epl.historical.execstrategy;
using com.espertech.esper.common.@internal.@event.bean.core;
using com.espertech.esper.common.@internal.metrics.audit;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.common.@internal.epl.historical.database.core
{
    /// <summary>
    ///     Viewable providing historical data from a database.
    /// </summary>
    public class PollExecStrategyDBQuery : PollExecStrategy
    {
        private static readonly ILog ADO_PERF_LOG = LogManager.GetLogger(AuditPath.ADO_LOG);
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly AgentInstanceContext agentInstanceContext;
        private readonly ConnectionCache connectionCache;

        private readonly HistoricalEventViewableDatabaseFactory factory;
        private Pair<DbDriver, DbDriverCommand> resources;

        public PollExecStrategyDBQuery(
            HistoricalEventViewableDatabaseFactory factory,
            AgentInstanceContext agentInstanceContext,
            ConnectionCache connectionCache)
        {
            this.factory = factory;
            this.agentInstanceContext = agentInstanceContext;
            this.connectionCache = connectionCache;
        }

        public void Start()
        {
            resources = connectionCache.Connection;
        }

        public void Done()
        {
            connectionCache.DoneWith(resources);
        }

        public void Dispose()
        {
            connectionCache.Dispose();
        }

        public void Destroy()
        {
            connectionCache.Dispose();
        }

        public IList<EventBean> Poll(
            object lookupValues,
            AgentInstanceContext agentInstanceContext)
        {
            IList<EventBean> result;
            try {
                result = Execute(resources.Second, lookupValues);
            }
            catch (EPException ex) {
                connectionCache.DoneWith(resources);
                throw;
            }

            return result;
        }

        private IList<EventBean> Execute(
            DbDriverCommand preparedStatement,
            object lookupValuePerStream)
        {
            lock (this) {
                var hasLogging = factory.enableLogging && ADO_PERF_LOG.IsInfoEnabled;

                // set parameters
                SQLInputParameterContext inputParameterContext = null;
                if (factory.columnTypeConversionHook != null) {
                    inputParameterContext = new SQLInputParameterContext();
                }

                var count = 1;
                object[] parameters = null;
                if (hasLogging) {
                    parameters = new object[factory.inputParameters.Length];
                }

                var mk = factory.inputParameters.Length == 1 ? null : (HashableMultiKey) lookupValuePerStream;
                for (var i = 0; i < factory.inputParameters.Length; i++) {
                    try {
                        object parameter;
                        if (mk == null) {
                            parameter = lookupValuePerStream;
                        }
                        else {
                            parameter = mk.Keys[i];
                        }

                        if (factory.columnTypeConversionHook != null) {
                            inputParameterContext.ParameterNumber = i + 1;
                            inputParameterContext.ParameterValue = parameter;
                            parameter = factory.columnTypeConversionHook.GetParameterValue(inputParameterContext);
                        }

                        SetObject(preparedStatement, count, parameter);
                        if (parameters != null) {
                            parameters[i] = parameter;
                        }
                    }
                    catch (SQLException ex) {
                        throw new EPException("Error setting parameter " + count, ex);
                    }

                    count++;
                }

                // execute
                ResultSet resultSet;
                if (hasLogging) {
                    long startTimeNS = PerformanceObserver.NanoTime;
                    long startTimeMS = PerformanceObserver.MilliTime;

                    try {
                        resultSet = preparedStatement.ExecuteQuery();
                    }
                    catch (SQLException ex) {
                        throw new EPException("Error executing statement '" + factory.preparedStatementText + '\'', ex);
                    }

                    long endTimeNS = PerformanceObserver.NanoTime;
                    long endTimeMS = PerformanceObserver.MilliTime;

                    ADO_PERF_LOG.Info(
                        "Statement '" + factory.preparedStatementText + "' delta nanosec " + (endTimeNS - startTimeNS) +
                        " delta msec " + (endTimeMS - startTimeMS) +
                        " parameters " + CompatExtensions.RenderAny(parameters));
                }
                else {
                    try {
                        resultSet = preparedStatement.ExecuteQuery();
                    }
                    catch (SQLException ex) {
                        throw new EPException("Error executing statement '" + factory.preparedStatementText + '\'', ex);
                    }
                }

                // generate events for result set
                IList<EventBean> rows = new List<EventBean>();
                try {
                    SQLColumnValueContext valueContext = null;
                    if (factory.columnTypeConversionHook != null) {
                        valueContext = new SQLColumnValueContext();
                    }

                    SQLOutputRowValueContext rowContext = null;
                    if (factory.outputRowConversionHook != null) {
                        rowContext = new SQLOutputRowValueContext();
                    }

                    var rowNum = 0;
                    while (resultSet.Next()) {
                        var colNum = 1;
                        IDictionary<string, object> row = new Dictionary<string, object>();
                        foreach (var entry in factory.outputTypes) {
                            var columnName = entry.Key;

                            object value;
                            var binding = entry.Value.OptionalBinding;
                            if (binding != null) {
                                value = binding.GetValue(resultSet, columnName);
                            }
                            else {
                                value = resultSet.GetObject(columnName);
                            }

                            if (factory.columnTypeConversionHook != null) {
                                valueContext.ColumnName = columnName;
                                valueContext.ColumnNumber = colNum;
                                valueContext.ColumnValue = value;
                                valueContext.ResultSet = resultSet;
                                value = factory.columnTypeConversionHook.GetColumnValue(valueContext);
                            }

                            row.Put(columnName, value);
                            colNum++;
                        }

                        EventBean eventBeanRow = null;
                        if (factory.outputRowConversionHook == null) {
                            eventBeanRow =
                                agentInstanceContext.EventBeanTypedEventFactory.AdapterForTypedMap(
                                    row, factory.EventType);
                        }
                        else {
                            rowContext.Values = row;
                            rowContext.RowNum = rowNum;
                            rowContext.ResultSet = resultSet;
                            var rowData = factory.outputRowConversionHook.GetOutputRow(rowContext);
                            if (rowData != null) {
                                eventBeanRow = agentInstanceContext.EventBeanTypedEventFactory.AdapterForTypedBean(
                                    rowData, (BeanEventType) factory.EventType);
                            }
                        }

                        if (eventBeanRow != null) {
                            rows.Add(eventBeanRow);
                            rowNum++;
                        }
                    }
                }
                catch (SQLException ex) {
                    throw new EPException(
                        "Error reading results for statement '" + factory.preparedStatementText + '\'', ex);
                }

                if (factory.enableLogging && ADO_PERF_LOG.IsInfoEnabled) {
                    ADO_PERF_LOG.Info("Statement '" + factory.preparedStatementText + "' " + rows.Count + " rows");
                }

                try {
                    resultSet.Close();
                }
                catch (SQLException ex) {
                    throw new EPException("Error closing statement '" + factory.preparedStatementText + '\'', ex);
                }

                return rows;
            }
        }

        private void SetObject(
            DbDriverCommand preparedStatement,
            int column,
            object value)
        {
            preparedStatement.SetObject(column, value);
        }
    }
} // end of namespace