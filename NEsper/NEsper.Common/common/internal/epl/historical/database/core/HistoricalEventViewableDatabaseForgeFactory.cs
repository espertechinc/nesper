///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.configuration.common;
using com.espertech.esper.common.client.hook.type;
using com.espertech.esper.common.client.meta;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.db;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.historical.database.connection;
using com.espertech.esper.common.@internal.@event.bean.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.common.@internal.epl.historical.database.core
{
    public class HistoricalEventViewableDatabaseForgeFactory
    {
        /// <summary>
        ///     Placeholder name for SQL-where clause substitution.
        /// </summary>
        public const string SAMPLE_WHERECLAUSE_PLACEHOLDER = "$ESPER-SAMPLE-WHERE";

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static HistoricalEventViewableDatabaseForge CreateDBStatementView(
            int streamNum,
            DBStatementStreamSpec databaseStreamSpec,
            SQLColumnTypeConversion columnTypeConversionHook,
            SQLOutputRowConversion outputRowConversionHook,
            StatementBaseInfo statementBaseInfo,
            StatementCompileTimeServices services,
            IEnumerable<Attribute> contextAttributes)
        {
            // Parse the SQL for placeholders and text fragments
            var sqlFragments = GetSqlFragments(databaseStreamSpec);
            var invocationInputParameters = new List<string>();
            foreach (var fragment in sqlFragments)
            {
                if ((fragment.IsParameter) && (fragment.Value != SAMPLE_WHERECLAUSE_PLACEHOLDER))
                {
                    invocationInputParameters.Add(fragment.Value);
                }
            }

            // Assemble a PreparedStatement and parameter list
            var preparedStatementText = CreatePreparedStatement(sqlFragments);
            var parameterDesc = GetParameters(sqlFragments);
            if (Log.IsDebugEnabled)
            {
                Log.Debug(
                    ".CreateDBStatementView preparedStatementText=" + preparedStatementText +
                    " parameterDesc=" + parameterDesc);
            }

            // Get a database connection
            var databaseName = databaseStreamSpec.DatabaseName;
            var dbDriver = services.DatabaseConfigServiceCompileTime
                .GetConnectionFactory(databaseName).Driver;
            var dbCommand = dbDriver.CreateCommand(
                sqlFragments,
                GetMetaDataSettings(services, databaseName),
                contextAttributes);

            if (Log.IsDebugEnabled)
            {
                Log.Debug(".CreateDBStatementView dbCommand=" + dbCommand);
            }

            var queryMetaData = GetQueryMetaData(
                databaseStreamSpec,
                services,
                dbCommand,
                parameterDesc,
                contextAttributes);

            Func<SQLColumnTypeContext, Type> columnTypeConversionFunc =
                columnTypeConversionHook != null
                    ? columnTypeConversionHook.GetColumnType
                    : (Func<SQLColumnTypeContext, Type>) null;
            Func<SQLOutputRowTypeContext, Type> outputRowConversionFunc =
                outputRowConversionHook != null
                    ? outputRowConversionHook.GetOutputRowType
                    : (Func<SQLOutputRowTypeContext, Type>) null;

            // Construct an event type from SQL query result metadata
            var eventType = CreateEventType(
                streamNum,
                queryMetaData,
                services,
                databaseStreamSpec,
                columnTypeConversionHook,
                outputRowConversionHook,
                statementBaseInfo);

            services.EventTypeCompileTimeRegistry.NewType(eventType);

            return new HistoricalEventViewableDatabaseForge(
                streamNum, eventType, databaseName,
                queryMetaData.InputParameters.ToArray(),
                preparedStatementText, queryMetaData.OutputParameters);
        }

        /// <summary>
        /// Creates an event type from the query meta data.
        /// </summary>
        /// <param name="streamNum"></param>
        /// <param name="queryMetaData">The query meta data.</param>
        /// <param name="services"></param>
        /// <param name="databaseStreamSpec">The database stream spec.</param>
        /// <param name="columnTypeConversionHook">The column type conversion hook.</param>
        /// <param name="outputRowConversionHook">The output row conversion hook.</param>
        /// <param name="base"></param>
        private static EventType CreateEventType(
            int streamNum,
            QueryMetaData queryMetaData,
            StatementCompileTimeServices services,
            DBStatementStreamSpec databaseStreamSpec,
            SQLColumnTypeConversion columnTypeConversionHook,
            SQLOutputRowConversion outputRowConversionHook,
            StatementBaseInfo @base)
        {
            var eventTypeFields = CreateEventTypeFields(
                databaseStreamSpec,
                columnTypeConversionHook,
                queryMetaData);

            var eventTypeName = services.EventTypeNameGeneratorStatement.GetAnonymousDBHistorical(streamNum);

            EventType eventType;
            Func<EventTypeApplicationType, EventTypeMetadata> metadata = appType => new EventTypeMetadata(
                eventTypeName, @base.ModuleName, EventTypeTypeClass.DBDERIVED, appType, NameAccessModifier.TRANSIENT,
                EventTypeBusModifier.NONBUS, false, EventTypeIdPair.Unassigned());
            if (outputRowConversionHook == null)
            {
                eventType = BaseNestableEventUtil.MakeMapTypeCompileTime(
                    metadata.Invoke(EventTypeApplicationType.MAP), eventTypeFields, null, null, null, null,
                    services.BeanEventTypeFactoryPrivate, services.EventTypeCompileTimeResolver);
            }
            else
            {
                var carrierClass = outputRowConversionHook.GetOutputRowType(
                    new SQLOutputRowTypeContext(
                        databaseStreamSpec.DatabaseName, databaseStreamSpec.SqlWithSubsParams, eventTypeFields));
                if (carrierClass == null)
                {
                    throw new ExprValidationException("Output row conversion hook returned no type");
                }

                var stem = services.BeanEventTypeStemService.GetCreateStem(carrierClass, null);
                eventType = new BeanEventType(
                    stem, metadata.Invoke(EventTypeApplicationType.CLASS), services.BeanEventTypeFactoryPrivate, null,
                    null, null, null);
            }

            return eventType;
        }

        /// <summary>
        /// Creates the event type fields.
        /// </summary>
        /// <param name="databaseStreamSpec">The database stream spec.</param>
        /// <param name="columnTypeConversionHook">The column type conversion hook.</param>
        /// <param name="queryMetaData">The query meta data.</param>
        /// <returns></returns>
        private static IDictionary<string, object> CreateEventTypeFields(
            DBStatementStreamSpec databaseStreamSpec,
            SQLColumnTypeConversion columnTypeConversionHook,
            QueryMetaData queryMetaData)
        {
            IDictionary<string, object> eventTypeFields = new Dictionary<string, object>();
            var columnNum = 1;
            foreach (var entry in queryMetaData.OutputParameters)
            {
                var name = entry.Key;
                var dbOutputDesc = entry.Value;

                Type clazz;
                if (dbOutputDesc.OptionalBinding != null)
                {
                    clazz = dbOutputDesc.OptionalBinding.DataType;
                }
                else
                {
                    clazz = dbOutputDesc.DataType;
                }

                if (columnTypeConversionHook != null)
                {
                    var newValue = columnTypeConversionHook.GetColumnType(
                        new SQLColumnTypeContext(
                            databaseStreamSpec.DatabaseName,
                            databaseStreamSpec.SqlWithSubsParams,
                            name, clazz,
                            dbOutputDesc.SqlType,
                            columnNum));

                    if (newValue != null)
                    {
                        clazz = newValue;
                    }
                }

                eventTypeFields.Put(name, clazz);
                columnNum++;
            }

            return eventTypeFields;
        }

        /// <summary>
        /// Gets the SQL fragments.
        /// </summary>
        /// <param name="databaseStreamSpec">The database stream spec.</param>
        /// <returns></returns>
        private static IList<PlaceholderParser.Fragment> GetSqlFragments(
            DBStatementStreamSpec databaseStreamSpec)
        {
            IList<PlaceholderParser.Fragment> sqlFragments;
            try
            {
                sqlFragments = PlaceholderParser.ParsePlaceholder(databaseStreamSpec.SqlWithSubsParams);
            }
            catch (PlaceholderParseException ex)
            {
                const string text = "Error parsing SQL";
                throw new ExprValidationException(text + ", reason: " + ex.Message, ex);
            }

            return sqlFragments;
        }

        /// <summary>
        /// Gets the query meta data.
        /// </summary>
        /// <param name="databaseStreamSpec">The database stream spec.</param>
        /// <param name="services"></param>
        /// <param name="dbCommand">The database command.</param>
        /// <param name="parameterDesc"></param>
        /// <returns></returns>

        private static QueryMetaData GetQueryMetaData(
            DBStatementStreamSpec databaseStreamSpec,
            StatementCompileTimeServices services,
            DbDriverCommand dbCommand,
            SQLParameterDesc parameterDesc,
            IEnumerable<Attribute> contextAttributes)
        {
            // On default setting, if we detect Oracle in the connection then don't query metadata from prepared statement
            var metadataSetting = dbCommand.MetaDataSettings;
            // On default setting, if we detect Oracle in the connection then don't query metadata from prepared statement
            var metaOriginPolicy = metadataSetting.MetadataRetrievalEnum;
            if (metaOriginPolicy == MetadataOriginEnum.DEFAULT)
            {
                // Ask the driver how it interprets the default meta origin policy; the
                // esper code has a specific hook for Oracle.  We have moved this into
                // the driver to avoid specifically coding behavior to a driver.
                metaOriginPolicy = dbCommand.Driver.DefaultMetaOriginPolicy;
            }

            QueryMetaData queryMetaData;

            switch (metaOriginPolicy)
            {
                case MetadataOriginEnum.METADATA:
                case MetadataOriginEnum.DEFAULT:
                    queryMetaData = dbCommand.MetaData;
                    // REWRITE: queryMetaData = GetPreparedStmtMetadata(
                    //    connection, parameterDesc.Parameters, preparedStatementText, metadataSetting);
                    break;

                case MetadataOriginEnum.SAMPLE:
                {
                    string sampleSQL;
                    var isGivenMetadataSQL = true;
                    if (databaseStreamSpec.MetadataSQL != null)
                    {
                        sampleSQL = databaseStreamSpec.MetadataSQL;
                        isGivenMetadataSQL = true;
                        if (Log.IsInfoEnabled)
                        {
                            Log.Info(".GetQueryMetaData Using provided sample SQL '" + sampleSQL + "'");
                        }
                    }
                    else
                    {
                        // Create the sample SQL by replacing placeholders with null and
                        // SAMPLE_WHERECLAUSE_PLACEHOLDER with a "where 1=0" clause

                        // REWRITE: sampleSQL = CreateSamplePlaceholderStatement(sqlFragments);
                        sampleSQL = CreateSamplePlaceholderStatement(dbCommand.Fragments);

                        if (Log.IsInfoEnabled)
                        {
                            Log.Info(".GetQueryMetaData Using un-lexed sample SQL '" + sampleSQL + "'");
                        }

                        // If there is no SAMPLE_WHERECLAUSE_PLACEHOLDER, lexical analyse the SQL
                        // adding a "where 1=0" clause.
                        if (parameterDesc.BuiltinIdentifiers.Count != 1)
                        {
                            sampleSQL = services.CompilerServices.LexSampleSQL(sampleSQL);
                            if (Log.IsInfoEnabled)
                            {
                                Log.Info(".GetQueryMetaData Using lexed sample SQL '" + sampleSQL + "'");
                            }
                        }
                    }

                    // finally get the metadata by firing the sample SQL
                    queryMetaData = GetExampleQueryMetaData(
                        dbCommand.Driver, sampleSQL, metadataSetting, contextAttributes);
                    break;
                }

                default:
                    throw new ArgumentException(
                        "MetaOriginPolicy contained an unhandled value: #" + metaOriginPolicy);
            }

            return queryMetaData;
        }

        /// <summary>
        /// Gets the example query meta data.
        /// </summary>
        /// <param name="dbDriver">The driver.</param>
        /// <param name="sampleSQL">The sample SQL.</param>
        /// <param name="metadataSetting">The metadata setting.</param>
        /// <param name="contextAttributes">The context attributes.</param>
        /// <returns></returns>
        private static QueryMetaData GetExampleQueryMetaData(
            DbDriver dbDriver,
            string sampleSQL,
            ColumnSettings metadataSetting,
            IEnumerable<Attribute> contextAttributes)
        {
            var sampleSQLFragments = PlaceholderParser.ParsePlaceholder(sampleSQL);
            using (var dbCommand = dbDriver.CreateCommand(sampleSQLFragments, metadataSetting, contextAttributes))
            {
                return dbCommand.MetaData;
            }
        }
        
        /// <summary>
        /// Gets the meta data settings from the database configuration service for the specified
        /// database name.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="databaseName"></param>
        /// <returns></returns>

        private static ColumnSettings GetMetaDataSettings(
            StatementCompileTimeServices services,
            String databaseName)
        {
            try
            {
                return services.DatabaseConfigServiceCompileTime.GetQuerySetting(databaseName);
            }
            catch (DatabaseConfigException ex)
            {
                var text = "Error connecting to database '" + databaseName + '\'';
                Log.Error(text, ex);
                throw new ExprValidationException(text + ", reason: " + ex.Message, ex);
            }
        }

        private static string CreatePreparedStatement(
            IEnumerable<PlaceholderParser.Fragment> parseFragements)
        {
            var buffer = new StringBuilder();
            foreach (var fragment in parseFragements)
            {
                if (!fragment.IsParameter)
                {
                    buffer.Append(fragment.Value);
                }
                else
                {
                    if (fragment.Value.Equals(SAMPLE_WHERECLAUSE_PLACEHOLDER))
                    {
                        continue;
                    }

                    buffer.Append('?');
                }
            }

            return buffer.ToString();
        }

        private static string CreateSamplePlaceholderStatement(
            IEnumerable<PlaceholderParser.Fragment> parseFragements)
        {
            var buffer = new StringBuilder();
            foreach (var fragment in parseFragements)
            {
                if (!fragment.IsParameter)
                {
                    buffer.Append(fragment.Value);
                }
                else
                {
                    if (fragment.Value.Equals(SAMPLE_WHERECLAUSE_PLACEHOLDER))
                    {
                        buffer.Append(" where 1=0 ");
                        break;
                    }

                    buffer.Append("null");
                }
            }

            return buffer.ToString();
        }

        private static SQLParameterDesc GetParameters(IList<PlaceholderParser.Fragment> parseFragements)
        {
            IList<string> eventPropertyParams = new List<string>();
            foreach (var fragment in parseFragements)
            {
                if (fragment.IsParameter && !fragment.Value.Equals(SAMPLE_WHERECLAUSE_PLACEHOLDER))
                {
                    eventPropertyParams.Add(fragment.Value);
                }
            }

            var parameters = eventPropertyParams.ToArray();
            var builtin = eventPropertyParams.ToArray();
            return new SQLParameterDesc(parameters, builtin);
        }
    }
} // end of namespace