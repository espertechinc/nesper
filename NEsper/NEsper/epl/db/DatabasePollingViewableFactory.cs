///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

using Antlr4.Runtime;

using com.espertech.esper.client;
using com.espertech.esper.compat.logging;
using com.espertech.esper.client.hook;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.parse;
using com.espertech.esper.epl.spec;
using com.espertech.esper.events;
using com.espertech.esper.util;
using com.espertech.esper.view;

namespace com.espertech.esper.epl.db
{
    /// <summary>
    /// Factory for a view onto historical data via SQL statement.
    /// </summary>

    public class DatabasePollingViewableFactory
    {
        public const String SAMPLE_WHERECLAUSE_PLACEHOLDER = "$ESPER-SAMPLE-WHERE";

        /// <summary>
        /// Creates the viewable for polling via database SQL query.
        /// </summary>
        /// <param name="statementId">The statement id.</param>
        /// <param name="streamNumber">is the stream number of the view</param>
        /// <param name="databaseStreamSpec">provides the SQL statement, database name and additional info</param>
        /// <param name="databaseConfigService">for getting database connection and settings</param>
        /// <param name="eventAdapterService">for generating event beans from database information</param>
        /// <param name="epStatementAgentInstanceHandle">The ep statement agent instance handle.</param>
        /// <param name="contextAttributes">The db attributes.</param>
        /// <param name="columnTypeConversionHook">The column type conversion hook.</param>
        /// <param name="outputRowConversionHook">The output row conversion hook.</param>
        /// <param name="enableAdoLogging">if set to <c>true</c> [enable JDBC logging].</param>
        /// <param name="dataCacheFactory">The data cache factory.</param>
        /// <param name="statementContext">The statement context.</param>
        /// <returns>
        /// viewable providing poll functionality
        /// </returns>
        /// <exception cref="ExprValidationException">the validation failed</exception>
        public static HistoricalEventViewable CreateDBStatementView(
            int statementId,
            int streamNumber,
            DBStatementStreamSpec databaseStreamSpec,
            DatabaseConfigService databaseConfigService,
            EventAdapterService eventAdapterService,
            EPStatementAgentInstanceHandle epStatementAgentInstanceHandle,
            IEnumerable<Attribute> contextAttributes,
            SQLColumnTypeConversion columnTypeConversionHook,
            SQLOutputRowConversion outputRowConversionHook,
            bool enableAdoLogging,
            DataCacheFactory dataCacheFactory,
            StatementContext statementContext)
        {
            // Parse the SQL for placeholders and text fragments
            var sqlFragments = GetSqlFragments(databaseStreamSpec);
            IList<String> invocationInputParameters = new List<string>();
            foreach (var fragment in sqlFragments)
            {
                if ((fragment.IsParameter) && (fragment.Value != SAMPLE_WHERECLAUSE_PLACEHOLDER))
                {
                    invocationInputParameters.Add(fragment.Value);
                }
            }

            // Get the database information
            var databaseName = databaseStreamSpec.DatabaseName;
            var dbDriver = GetDatabaseConnectionFactory(databaseConfigService, databaseName).Driver;
            var dbCommand = dbDriver.CreateCommand(
                sqlFragments,
                GetMetaDataSettings(databaseConfigService, databaseName),
                contextAttributes);

            if (Log.IsDebugEnabled)
            {
                Log.Debug(".CreateDBStatementView dbCommand=" + dbCommand);
            }

            var queryMetaData = GetQueryMetaData(
                databaseStreamSpec,
                databaseConfigService,
                dbCommand,
                contextAttributes);

            Func<SQLColumnTypeContext, Type> columnTypeConversionFunc = null;
            if (columnTypeConversionHook != null)
            {
                columnTypeConversionFunc = columnTypeConversionHook.GetColumnType;
            }

            Func<SQLOutputRowTypeContext, Type> outputRowConversionFunc = null;
            if (outputRowConversionHook != null)
            {
                outputRowConversionFunc = outputRowConversionHook.GetOutputRowType;
            }

            // Construct an event type from SQL query result metadata
            var eventType = CreateEventType(
                statementId,
                streamNumber,
                queryMetaData,
                eventAdapterService,
                databaseStreamSpec,
                columnTypeConversionFunc,
                outputRowConversionFunc);

            // Get a proper connection and data cache
            ConnectionCache connectionCache;
            DataCache dataCache;
            try
            {
                connectionCache = databaseConfigService.GetConnectionCache(
                    databaseName, dbCommand.PseudoText, contextAttributes);
                dataCache = databaseConfigService.GetDataCache(
                    databaseName, statementContext, epStatementAgentInstanceHandle, dataCacheFactory, streamNumber);
            }
            catch (DatabaseConfigException e)
            {
                const string text = "Error obtaining cache configuration";
                Log.Error(text, e);
                throw new ExprValidationException(text + ", reason: " + e.Message, e);
            }

            var dbPollStrategy = new PollExecStrategyDBQuery(
                eventAdapterService,
                eventType,
                connectionCache,
                dbCommand.CommandText,
                queryMetaData.OutputParameters,
                columnTypeConversionHook,
                outputRowConversionHook);

            return new DatabasePollingViewable(
                streamNumber,
                invocationInputParameters,
                dbPollStrategy,
                dataCache,
                eventType,
                statementContext.ThreadLocalManager);
        }

        /// <summary>
        /// Gets the meta data settings from the database configuration service for the specified
        /// database name.
        /// </summary>
        /// <param name="databaseConfigService"></param>
        /// <param name="databaseName"></param>
        /// <returns></returns>

        private static ColumnSettings GetMetaDataSettings(
            DatabaseConfigService databaseConfigService,
            String databaseName)
        {
            try
            {
                return databaseConfigService.GetQuerySetting(databaseName);
            }
            catch (DatabaseConfigException ex)
            {
                var text = "Error connecting to database '" + databaseName + '\'';
                Log.Error(text, ex);
                throw new ExprValidationException(text + ", reason: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Creates an event type from the query meta data.
        /// </summary>
        /// <param name="statementId">The statement id.</param>
        /// <param name="streamNumber">The stream number.</param>
        /// <param name="queryMetaData">The query meta data.</param>
        /// <param name="eventAdapterService">The event adapter service.</param>
        /// <param name="databaseStreamSpec">The database stream spec.</param>
        /// <param name="columnTypeConversionHook">The column type conversion hook.</param>
        /// <param name="outputRowConversionHook">The output row conversion hook.</param>
        /// <returns></returns>
        private static EventType CreateEventType(
            int statementId,
            int streamNumber,
            QueryMetaData queryMetaData,
            EventAdapterService eventAdapterService,
            DBStatementStreamSpec databaseStreamSpec,
            Func<SQLColumnTypeContext, Type> columnTypeConversionHook,
            Func<SQLOutputRowTypeContext, Type> outputRowConversionHook)
        {
            var columnNum = 1;
            var eventTypeFields = new Dictionary<String, Object>();
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

                    var newValue = columnTypeConversionHook.Invoke(
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

                eventTypeFields[name] = clazz.GetBoxedType();
                columnNum++;
            }

            EventType eventType;
            if (outputRowConversionHook == null)
            {
                var outputEventType = statementId + "_dbpoll_" + streamNumber;
                eventType = eventAdapterService.CreateAnonymousMapType(outputEventType, eventTypeFields, true);
            }
            else
            {
                var carrierClass = outputRowConversionHook.Invoke(
                    new SQLOutputRowTypeContext(
                        databaseStreamSpec.DatabaseName,
                        databaseStreamSpec.SqlWithSubsParams,
                        eventTypeFields));
                if (carrierClass == null)
                {
                    throw new ExprValidationException("Output row conversion hook returned no type");
                }

                eventType = eventAdapterService.AddBeanType(carrierClass.FullName, carrierClass, false, false, false);
            }

            return eventType;
        }

        /// <summary>
        /// Gets the database connection factory.
        /// </summary>
        /// <param name="databaseConfigService">The database config service.</param>
        /// <param name="databaseName">Name of the database.</param>
        /// <returns></returns>
        private static DatabaseConnectionFactory GetDatabaseConnectionFactory(
            DatabaseConfigService databaseConfigService,
            string databaseName)
        {
            DatabaseConnectionFactory databaseConnectionFactory;
            try
            {
                databaseConnectionFactory = databaseConfigService.GetConnectionFactory(databaseName);
            }
            catch (DatabaseConfigException ex)
            {
                var text = "Error connecting to database '" + databaseName + "'";
                Log.Error(text, ex);
                throw new ExprValidationException(text + ", reason: " + ex.Message, ex);
            }
            return databaseConnectionFactory;
        }

        /// <summary>
        /// Gets the SQL fragments.
        /// </summary>
        /// <param name="databaseStreamSpec">The database stream spec.</param>
        /// <returns></returns>
        private static IList<PlaceholderParser.Fragment> GetSqlFragments(DBStatementStreamSpec databaseStreamSpec)
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
        /// <param name="databaseConfigService">The database config service.</param>
        /// <param name="dbCommand">The database command.</param>
        /// <param name="contextAttributes">The context attributes.</param>
        /// <returns></returns>
        private static QueryMetaData GetQueryMetaData(
            DBStatementStreamSpec databaseStreamSpec,
            DatabaseConfigService databaseConfigService,
            DbDriverCommand dbCommand,
            IEnumerable<Attribute> contextAttributes)
        {
            // Get a database connection
            var databaseName = databaseStreamSpec.DatabaseName;
            //DatabaseConnectionFactory databaseConnectionFactory = GetDatabaseConnectionFactory(databaseConfigService, databaseName);
            var metadataSetting = dbCommand.MetaDataSettings;

            QueryMetaData queryMetaData;
            try
            {
                // On default setting, if we detect Oracle in the connection then don't query metadata from prepared statement
                var metaOriginPolicy = metadataSetting.MetadataRetrievalEnum;
                if (metaOriginPolicy == ConfigurationDBRef.MetadataOriginEnum.DEFAULT)
                {
                    // Ask the driver how it interprets the default meta origin policy; the
                    // esper code has a specific hook for Oracle.  We have moved this into
                    // the driver to avoid specifically coding behavior to a driver.
                    metaOriginPolicy = dbCommand.Driver.DefaultMetaOriginPolicy;
                }

                switch (metaOriginPolicy)
                {
                    case ConfigurationDBRef.MetadataOriginEnum.METADATA:
                    case ConfigurationDBRef.MetadataOriginEnum.DEFAULT:
                        queryMetaData = dbCommand.GetMetaData();
                        break;
                    case ConfigurationDBRef.MetadataOriginEnum.SAMPLE:
                    {
                        var parameterDesc = dbCommand.ParameterDescription;

                        String sampleSQL;
                        if (databaseStreamSpec.MetadataSQL != null)
                        {
                            sampleSQL = databaseStreamSpec.MetadataSQL;
                            if (Log.IsInfoEnabled)
                            {
                                Log.Info(".GetQueryMetaData Using provided sample SQL '" + sampleSQL + "'");
                            }
                        }
                        else
                        {
                            // Create the sample SQL by replacing placeholders with null and
                            // SAMPLE_WHERECLAUSE_PLACEHOLDER with a "where 1=0" clause
                            sampleSQL = CreateSamplePlaceholderStatement(dbCommand.Fragments);

                            if (Log.IsInfoEnabled)
                            {
                                Log.Info(".GetQueryMetaData Using un-lexed sample SQL '" + sampleSQL + "'");
                            }

                            // If there is no SAMPLE_WHERECLAUSE_PLACEHOLDER, lexical analyse the SQL
                            // adding a "where 1=0" clause.
                            if (parameterDesc.BuiltinIdentifiers.Count != 1)
                            {
                                sampleSQL = LexSampleSQL(sampleSQL);
                                if (Log.IsInfoEnabled)
                                {
                                    Log.Info(".GetQueryMetaData Using lexed sample SQL '" + sampleSQL + "'");
                                }
                            }
                        }

                        // finally get the metadata by firing the sample SQL
                        queryMetaData = GetExampleQueryMetaData(
                            dbCommand.Driver, sampleSQL, metadataSetting, contextAttributes);
                    }
                        break;
                    default:
                        throw new ArgumentException(
                            "MetaOriginPolicy contained an unhandled value: #" + metaOriginPolicy);
                }
            }
            catch (DatabaseConfigException ex)
            {
                var text = "Error connecting to database '" + databaseName + '\'';
                Log.Error(text, ex);
                throw new ExprValidationException(text + ", reason: " + ex.Message, ex);
            }

            return queryMetaData;
        }

        /// <summary>
        /// Lexes the sample SQL and inserts a "where 1=0" where-clause.
        /// </summary>
        /// <param name="querySQL">to inspect using lexer</param>
        /// <returns>sample SQL with where-clause inserted</returns>
        /// <exception cref="ExprValidationException">indicates a lexer problem</exception>
        public static String LexSampleSQL(String querySQL)
        {
            querySQL = Regex.Replace(querySQL, "\\s\\s+|\\n|\\r", m => " ");

            ICharStream input;
            try
            {
                input = new NoCaseSensitiveStream(querySQL);
            }
            catch (IOException ex)
            {
                throw new ExprValidationException("IOException lexing query SQL '" + querySQL + '\'', ex);
            }

            var whereIndex = -1;
            var groupbyIndex = -1;
            var havingIndex = -1;
            var orderByIndex = -1;
            var unionIndexes = new List<int>();

            var lex = ParseHelper.NewLexer(input);
            var tokens = new CommonTokenStream(lex);
            tokens.Fill();

            IList<IToken> tokenList = tokens.GetTokens();

            for (var i = 0; i < tokenList.Count; i++)
            {
                var token = (IToken) tokenList[i];
                if ((token == null) || token.Text == null)
                {
                    break;
                }
                var text = token.Text.ToLower().Trim();
                if (text == "")
                {
                    continue;
                }

                switch (text)
                {
                    case "where":
                        whereIndex = token.Column + 1;
                        break;
                    case "group":
                        groupbyIndex = token.Column + 1;
                        break;
                    case "having":
                        havingIndex = token.Column + 1;
                        break;
                    case "order":
                        orderByIndex = token.Column + 1;
                        break;
                    case "union":
                        unionIndexes.Add(token.Column + 1);
                        break;
                }
            }

            // If we have a union, break string into subselects and process each
            if (unionIndexes.Count != 0)
            {
                String fragment;
                String lexedFragment;
                var changedSQL = new StringBuilder();
                var lastIndex = 0;
                for (var i = 0; i < unionIndexes.Count; i++)
                {
                    var index = unionIndexes[i];
                    if (i > 0)
                    {
                        fragment = querySQL.Substring(lastIndex + 5, index - 6 - lastIndex);
                    }
                    else
                    {
                        fragment = querySQL.Substring(lastIndex, index - 1 - lastIndex);
                    }

                    lexedFragment = LexSampleSQL(fragment);

                    if (i > 0)
                    {
                        changedSQL.Append("union ");
                    }
                    changedSQL.Append(lexedFragment);
                    lastIndex = index - 1;
                }

                // last part after last union
                fragment = querySQL.Substring(lastIndex + 5);
                lexedFragment = LexSampleSQL(fragment);
                changedSQL.Append("union ");
                changedSQL.Append(lexedFragment);

                return changedSQL.ToString();
            }

            // Found a where clause, simplest cases
            if (whereIndex != -1)
            {
                var changedSQL = new StringBuilder();
                var prefix = querySQL.Substring(0, whereIndex + 5);
                var suffix = querySQL.Substring(whereIndex + 5);
                changedSQL.Append(prefix);
                changedSQL.Append("1=0 and ");
                changedSQL.Append(suffix);
                return changedSQL.ToString();
            }

            // No where clause, find group-by
            int insertIndex;
            if (groupbyIndex != -1)
            {
                insertIndex = groupbyIndex;
            }
            else if (havingIndex != -1)
            {
                insertIndex = havingIndex;
            }
            else if (orderByIndex != -1)
            {
                insertIndex = orderByIndex;
            }
            else
            {
                var changedSQL = new StringBuilder();
                changedSQL.Append(querySQL);
                changedSQL.Append(" where 1=0 ");
                return changedSQL.ToString();
            }

            try
            {
                var changedSQL = new StringBuilder();
                var prefix = querySQL.Substring(0, insertIndex - 1);
                changedSQL.Append(prefix);
                changedSQL.Append("where 1=0 ");
                var suffix = querySQL.Substring(insertIndex - 1);
                changedSQL.Append(suffix);
                return changedSQL.ToString();
            }
            catch (Exception ex)
            {
                const string text =
                    "Error constructing sample SQL to retrieve metadata for JDBC-drivers that don't support metadata, consider using the " +
                    SAMPLE_WHERECLAUSE_PLACEHOLDER + " placeholder or providing a sample SQL";
                Log.Error(text, ex);
                throw new ExprValidationException(text, ex);
            }
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
            String sampleSQL,
            ColumnSettings metadataSetting,
            IEnumerable<Attribute> contextAttributes)
        {
            var sampleSQLFragments = PlaceholderParser.ParsePlaceholder(sampleSQL);
            using (var dbCommand = dbDriver.CreateCommand(sampleSQLFragments, metadataSetting, contextAttributes))
            {
                return dbCommand.GetMetaData();
            }
        }

        /// <summary>
        /// Creates the sample placeholder statement.
        /// </summary>
        /// <param name="parseFragements">The parse fragements.</param>
        /// <returns></returns>
        private static String CreateSamplePlaceholderStatement(IEnumerable<PlaceholderParser.Fragment> parseFragements)
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
                    else
                    {
                        buffer.Append("null");
                    }
                }
            }
            return buffer.ToString();
        }

        private static readonly ILog Log =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
