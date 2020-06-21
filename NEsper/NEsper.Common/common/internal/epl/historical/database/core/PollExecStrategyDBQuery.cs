///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Data.Common;
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
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

using DataMap = System.Collections.Generic.IDictionary<string, object>;

namespace com.espertech.esper.common.@internal.epl.historical.database.core
{
    /// <summary>
    ///     Viewable providing historical data from a database.
    /// </summary>
    public class PollExecStrategyDBQuery : PollExecStrategy
    {
        private static readonly ILog ADO_PERF_LOG = LogManager.GetLogger(AuditPath.ADO_LOG);
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly AgentInstanceContext _agentInstanceContext;
        private readonly ConnectionCache _connectionCache;
        private readonly HistoricalEventViewableDatabaseFactory _factory;
        private Pair<DbDriver, DbDriverCommand> _resources;

        private readonly SQLColumnTypeConversion _columnTypeConversionHook;
        private readonly SQLOutputRowConversion _outputRowConversionHook;
        private readonly IDictionary<string, DBOutputTypeDesc> _outputTypes;
        private List<DbInfo> _dbInfoList;

        public PollExecStrategyDBQuery(
            HistoricalEventViewableDatabaseFactory factory,
            AgentInstanceContext agentInstanceContext,
            ConnectionCache connectionCache)
        {
            _factory = factory;
            _agentInstanceContext = agentInstanceContext;
            _connectionCache = connectionCache;
            _dbInfoList = null;
            _outputTypes = factory.OutputTypes;
            _columnTypeConversionHook = factory.ColumnTypeConversionHook;
            _outputRowConversionHook = factory.OutputRowConversionHook;
        }

        /// <summary>
        /// Start the poll, called before any poll operation.
        /// </summary>
        public virtual void Start()
        {
            _resources = _connectionCache.Connection;
        }

        /// <summary>
        /// Indicate we are done polling and can release resources.
        /// </summary>
        public virtual void Done()
        {
            _connectionCache.DoneWith(_resources);
        }

        /// <summary>
        /// Indicate we are no going to use this object again.
        /// </summary>
        public void Dispose()
        {
            _connectionCache.Dispose();
        }

        public void Destroy()
        {
            _connectionCache.Dispose();
        }

        /// <summary>
        /// Poll events using the keys provided.
        /// </summary>
        /// <param name="lookupValues">is keys for executing a query or such</param>
        /// <param name="agentInstanceContext">The agent instance context.</param>
        /// <returns>
        /// a list of events for the keys
        /// </returns>
        public IList<EventBean> Poll(
            object lookupValues,
            AgentInstanceContext agentInstanceContext)
        {
            IList<EventBean> result;
            try {
                result = Execute(_resources.Second, lookupValues);
            }
            catch (EPException) {
                _connectionCache.DoneWith(_resources);
                throw;
            }

            return result;
        }

        private IList<EventBean> Execute(
            DbDriverCommand driverCommand,
            object lookupValuePerStream)
        {
            var hasLogging = _factory.IsEnableLogging && ADO_PERF_LOG.IsInfoEnabled;

            using (var myDriverCommand = driverCommand.Clone()) {
                var dbCommand = myDriverCommand.Command;

                if (ExecutionPathDebugLog.IsDebugEnabled && Log.IsInfoEnabled) {
                    Log.Info(".execute Executing prepared statement '{0}'", dbCommand.CommandText);
                }

                DbParameter dbParam;

                // set parameters
                SQLInputParameterContext inputParameterContext = null;
                if (_columnTypeConversionHook != null) {
                    inputParameterContext = new SQLInputParameterContext();
                }

                var mk = _factory.InputParameters.Length == 1 ? null : (object[]) lookupValuePerStream;
                for (var i = 0; i < _factory.InputParameters.Length; i++) {
                    try {
                        object parameter;
                        if (mk == null) {
                            parameter = lookupValuePerStream;
                        }
                        else {
                            parameter = mk[i];
                        }

                        if (ExecutionPathDebugLog.IsDebugEnabled && Log.IsInfoEnabled) {
                            Log.Info(
                                ".Execute Setting parameter " +
                                " to " +
                                parameter +
                                " typed " +
                                ((parameter == null) ? "null" : parameter.GetType().Name));
                        }

                        if (_columnTypeConversionHook != null) {
                            inputParameterContext.ParameterNumber = i + 1;
                            inputParameterContext.ParameterValue = parameter;
                            parameter = _columnTypeConversionHook.GetParameterValue(inputParameterContext);
                        }

                        dbParam = dbCommand.Parameters[i];
                        dbParam.Value = parameter ?? DBNull.Value;
                    }
                    catch (DbException ex) {
                        throw new EPException("Error setting parameter " + i, ex);
                    }
                }

                // execute
                try {
                    // generate events for result set
                    IList<EventBean> rows = new List<EventBean>();

                    using (var dataReader = dbCommand.ExecuteReader()) {
                        try {
                            SQLColumnValueContext valueContext = null;
                            if (_columnTypeConversionHook != null) {
                                valueContext = new SQLColumnValueContext();
                            }

                            SQLOutputRowValueContext rowContext = null;
                            if (_outputRowConversionHook != null) {
                                rowContext = new SQLOutputRowValueContext();
                            }

                            var rowNum = 0;

                            if (dataReader.HasRows) {
                                // Determine how many fields we will be receiving
                                var fieldCount = dataReader.FieldCount;
                                // Allocate a buffer to hold the results of the row
                                var rawData = new object[fieldCount];
                                // Convert the names of columns into ordinal indices and prepare
                                // them so that we only have to incur this cost when we first notice
                                // the reader has rows.
                                if (_dbInfoList == null) {
                                    _dbInfoList = new List<DbInfo>();
                                    foreach (var entry in _outputTypes) {
                                        var dbInfo = new DbInfo();
                                        dbInfo.Name = entry.Key;
                                        dbInfo.Ordinal = dataReader.GetOrdinal(dbInfo.Name);
                                        dbInfo.OutputTypeDesc = entry.Value;
                                        dbInfo.Binding = entry.Value.OptionalBinding;
                                        _dbInfoList.Add(dbInfo);
                                    }
                                }

                                var fieldNames = new string[fieldCount];
                                for (var ii = 0; ii < fieldCount; ii++) {
                                    fieldNames[ii] = dataReader.GetName(ii);
                                }

                                // Anyone know if the ordinal will always be the same every time
                                // the query is executed; if so, we could certainly cache this
                                // dbInfoList so that we only have to do that once for the lifetime
                                // of the statement.
                                while (dataReader.Read()) {
                                    var colNum = 1;

                                    DataMap row = new Dictionary<string, object>();
                                    // Get all of the values for the row in one shot
                                    dataReader.GetValues(rawData);
                                    // Convert the items into raw row objects
                                    foreach (var dbInfo in _dbInfoList) {
                                        var value = rawData[dbInfo.Ordinal];
                                        if (value == DBNull.Value) {
                                            value = null;
                                        }
                                        else if (dbInfo.Binding != null) {
                                            value = dbInfo.Binding.GetValue(value, dbInfo.Name);
                                        }
                                        else if (value.GetType() != dbInfo.OutputTypeDesc.DataType) {
                                            value = Convert.ChangeType(value, dbInfo.OutputTypeDesc.DataType);
                                        }

                                        if (_columnTypeConversionHook != null) {
                                            valueContext.ColumnName = fieldNames[colNum - 1];
                                            valueContext.ColumnNumber = colNum;
                                            valueContext.ColumnValue = value;

                                            value = _columnTypeConversionHook.GetColumnValue(valueContext);
                                        }

                                        row[dbInfo.Name] = value;

                                        colNum++;
                                    }

                                    EventBean eventBeanRow = null;
                                    if (_outputRowConversionHook == null) {
                                        eventBeanRow = _agentInstanceContext.EventBeanTypedEventFactory
                                            .AdapterForTypedMap(
                                                row,
                                                _factory.EventType);
                                    }
                                    else {
                                        rowContext.Values = row;
                                        rowContext.RowNum = rowNum;
                                        var rowData = _outputRowConversionHook.GetOutputRow(rowContext);
                                        if (rowData != null) {
                                            eventBeanRow = _agentInstanceContext.EventBeanTypedEventFactory
                                                .AdapterForTypedObject(
                                                    rowData,
                                                    (BeanEventType) _factory.EventType);
                                        }
                                    }

                                    if (eventBeanRow != null) {
                                        rows.Add(eventBeanRow);
                                        rowNum++;
                                    }
                                }
                            }
                        }
                        catch (DbException ex) {
                            throw new EPException(
                                "Error reading results for statement '" + _factory.PreparedStatementText + "'",
                                ex);
                        }
                    }

                    return rows;
                }
                catch (DbException ex) {
                    throw new EPException("Error executing statement '" + _factory.PreparedStatementText + "'", ex);
                }
            }
        }

        private struct DbInfo
        {
            public string Name;
            public int Ordinal;
            public DBOutputTypeDesc OutputTypeDesc;
            public DatabaseTypeBinding Binding;
        }
    }
} // end of namespace