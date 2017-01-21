///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;
using System.Data.Common;

using com.espertech.esper.client;
using com.espertech.esper.compat.logging;
using com.espertech.esper.client.hook;
using com.espertech.esper.collection;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.events;
using com.espertech.esper.events.bean;
using com.espertech.esper.util;

using DataMap = System.Collections.Generic.IDictionary<string, object>;

namespace com.espertech.esper.epl.db
{
    /// <summary>
    /// Viewable providing historical data from a database.
    /// </summary>

    public class PollExecStrategyDBQuery : PollExecStrategy
    {
        private readonly EventAdapterService _eventAdapterService;
        private readonly String _preparedStatementText;
        private readonly IDictionary<String, DBOutputTypeDesc> _outputTypes;
        private readonly ConnectionCache _connectionCache;
        private readonly EventType _eventType;
        private readonly SQLColumnTypeConversion _columnTypeConversionHook;
        private readonly SQLOutputRowConversion _outputRowConversionHook;

        private Pair<DbDriver, DbDriverCommand> _resources;

        /// <summary>Ctor. </summary>
        /// <param name="eventAdapterService">for generating event beans</param>
        /// <param name="eventType">is the event type that this poll generates</param>
        /// <param name="connectionCache">caches Connection and PreparedStatement</param>
        /// <param name="preparedStatementText">is the SQL to use for polling</param>
        /// <param name="outputTypes">describe columns selected by the SQL</param>
        /// <param name="outputRowConversionHook">hook to convert rows, if any hook is registered</param>
        /// <param name="columnTypeConversionHook">hook to convert columns, if any hook is registered</param>
        public PollExecStrategyDBQuery(EventAdapterService eventAdapterService,
                                       EventType eventType,
                                       ConnectionCache connectionCache,
                                       String preparedStatementText,
                                       IDictionary<String, DBOutputTypeDesc> outputTypes,
                                       SQLColumnTypeConversion columnTypeConversionHook,
                                       SQLOutputRowConversion outputRowConversionHook)
        {
            _eventAdapterService = eventAdapterService;
            _eventType = eventType;
            _connectionCache = connectionCache;
            _preparedStatementText = preparedStatementText;
            _outputTypes = outputTypes;
            _columnTypeConversionHook = columnTypeConversionHook;
            _outputRowConversionHook = outputRowConversionHook;
        }

        /// <summary>
        /// Start the poll, called before any poll operation.
        /// </summary>
        public virtual void Start()
        {
            _resources = _connectionCache.GetConnection();
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
        public virtual void Dispose()
        {
            _connectionCache.Dispose();
        }

        /// <summary>
        /// Poll events using the keys provided.
        /// </summary>
        /// <param name="lookupValues">is keys for exeuting a query or such</param>
        /// <returns>a list of events for the keys</returns>
        public IList<EventBean> Poll(Object[] lookupValues, ExprEvaluatorContext exprEvaluatorContext)
        {
            IList<EventBean> result;
            try
            {
                result = Execute(_resources.Second, lookupValues);
            }
            catch (EPException)
            {
                _connectionCache.DoneWith(_resources);
                throw;
            }

            return result;
        }

        private List<DbInfo> _dbInfoList = null;

        private IList<EventBean> Execute(DbDriverCommand driverCommand, Object[] lookupValuePerStream)
        {
            using (DbDriverCommand myDriverCommand = driverCommand.Clone())
            {
                DbCommand dbCommand = myDriverCommand.Command;

                if (ExecutionPathDebugLog.IsEnabled && Log.IsInfoEnabled)
                {
                    Log.Info(".execute Executing prepared statement '{0}'", dbCommand.CommandText);
                }

                int dbParamCount = dbCommand.Parameters.Count;
                if (dbParamCount != lookupValuePerStream.Length)
                {
                    throw new ArgumentException("Only those parameters that have been prepared may be used here");
                }

                DbParameter dbParam;

                // set parameters
                SQLInputParameterContext inputParameterContext = null;
                if (_columnTypeConversionHook != null)
                {
                    inputParameterContext = new SQLInputParameterContext();
                }

                for (int i = 0; i < lookupValuePerStream.Length; i++)
                {
                    try
                    {
                        Object parameter = lookupValuePerStream[i];
                        if (ExecutionPathDebugLog.IsEnabled && Log.IsInfoEnabled)
                        {
                            Log.Info(".Execute Setting parameter " + " to " + parameter + " typed " +
                                     ((parameter == null) ? "null" : parameter.GetType().Name));
                        }

                        if (_columnTypeConversionHook != null)
                        {
                            inputParameterContext.ParameterNumber = i + 1;
                            inputParameterContext.ParameterValue = parameter;
                            parameter = _columnTypeConversionHook.GetParameterValue(inputParameterContext);
                        }

                        dbParam = dbCommand.Parameters[i];
                        dbParam.Value = parameter ?? DBNull.Value;
                    }
                    catch (DbException ex)
                    {
                        throw new EPException("Error setting parameter " + i, ex);
                    }
                }

                // execute
                try
                {
                    // generate events for result set
                    IList<EventBean> rows = new List<EventBean>();

                    using (DbDataReader dataReader = dbCommand.ExecuteReader())
                    {
                        try
                        {
                            SQLColumnValueContext valueContext = null;
                            if (_columnTypeConversionHook != null)
                            {
                                valueContext = new SQLColumnValueContext();
                            }

                            SQLOutputRowValueContext rowContext = null;
                            if (_outputRowConversionHook != null)
                            {
                                rowContext = new SQLOutputRowValueContext();
                            }

                            int rowNum = 0;

                            if (dataReader.HasRows)
                            {
                                // Determine how many fields we will be receiving
                                int fieldCount = dataReader.FieldCount;
                                // Allocate a buffer to hold the results of the row
                                Object[] rawData = new object[fieldCount];
                                // Convert the names of columns into ordinal indices and prepare
                                // them so that we only have to incur this cost when we first notice
                                // the reader has rows.
                                if (_dbInfoList == null)
                                {
                                    _dbInfoList = new List<DbInfo>();
                                    foreach (KeyValuePair<String, DBOutputTypeDesc> entry in _outputTypes)
                                    {
                                        DbInfo dbInfo = new DbInfo();
                                        dbInfo.Name = entry.Key;
                                        dbInfo.Ordinal = dataReader.GetOrdinal(dbInfo.Name);
                                        dbInfo.OutputTypeDesc = entry.Value;
                                        dbInfo.Binding = entry.Value.OptionalBinding;
                                        _dbInfoList.Add(dbInfo);
                                    }
                                }

                                var fieldNames = new string[fieldCount];
                                for (int ii = 0; ii < fieldCount; ii++)
                                {
                                    fieldNames[ii] = dataReader.GetName(ii);
                                }

                                // Anyone know if the ordinal will always be the same every time
                                // the query is executed; if so, we could certainly cache this
                                // dbInfoList so that we only have to do that once for the lifetime
                                // of the statement.
                                while (dataReader.Read())
                                {
                                    int colNum = 1;

                                    DataMap row = new Dictionary<string, object>();
                                    // Get all of the values for the row in one shot
                                    dataReader.GetValues(rawData);
                                    // Convert the items into raw row objects
                                    foreach (DbInfo dbInfo in _dbInfoList)
                                    {
                                        Object value = rawData[dbInfo.Ordinal];
                                        if (value == DBNull.Value)
                                        {
                                            value = null;
                                        }
                                        else if (dbInfo.Binding != null)
                                        {
                                            value = dbInfo.Binding.GetValue(value, dbInfo.Name);
                                        }
                                        else if (value.GetType() != dbInfo.OutputTypeDesc.DataType)
                                        {
                                            value = Convert.ChangeType(value, dbInfo.OutputTypeDesc.DataType);
                                        }

                                        if (_columnTypeConversionHook != null)
                                        {
                                            valueContext.ColumnName = fieldNames[colNum - 1];
                                            valueContext.ColumnNumber = colNum;
                                            valueContext.ColumnValue = value;

                                            value = _columnTypeConversionHook.GetColumnValue(valueContext);
                                        }

                                        row[dbInfo.Name] = value;

                                        colNum++;
                                    }

                                    EventBean eventBeanRow = null;
                                    if (_outputRowConversionHook == null)
                                    {
                                        eventBeanRow = _eventAdapterService.AdapterForTypedMap(row, _eventType);
                                    }
                                    else
                                    {
                                        rowContext.Values = row;
                                        rowContext.RowNum = rowNum;
                                        Object rowData = _outputRowConversionHook.GetOutputRow(rowContext);
                                        if (rowData != null)
                                        {
                                            eventBeanRow = _eventAdapterService.AdapterForTypedObject(rowData, (BeanEventType)_eventType);
                                        }
                                    }

                                    if (eventBeanRow != null)
                                    {
                                        rows.Add(eventBeanRow);
                                        rowNum++;
                                    }
                                }
                            }
                        }
                        catch (DbException ex)
                        {
                            throw new EPException(
                                "Error reading results for statement '" + _preparedStatementText + "'", ex);
                        }
                    }


                    return rows;
                }
                catch (DbException ex)
                {
                    throw new EPException("Error executing statement '" + _preparedStatementText + "'", ex);
                }
            }
        }

        struct DbInfo
        {
            public string Name;
            public int Ordinal;
            public DBOutputTypeDesc OutputTypeDesc;
            public DatabaseTypeBinding Binding;
        }

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
