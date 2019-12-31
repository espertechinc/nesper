///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;

using com.espertech.esper.common.client.configuration.common;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.historical.database.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.common.@internal.db.drivers
{
    /// <summary>
    /// Companion to the BaseDbDriver that provides command support in
    /// accordance to ADO.NET and the DbDriverCommand.
    /// </summary>
    public class BaseDbDriverCommand : DbDriverCommand
    {
        private const string SAMPLE_WHERECLAUSE_PLACEHOLDER = "$ESPER-SAMPLE-WHERE";

        /// <summary>
        /// Underlying driver.
        /// </summary>
        private readonly BaseDbDriver _driver;

        /// <summary>
        /// Fragments that were used to build the command.
        /// </summary>
        private readonly List<PlaceholderParser.Fragment> _fragments;

        /// <summary>
        /// List of input parameters
        /// </summary>
        private readonly List<string> _inputParameters;

        /// <summary>
        /// Output parameters; cached upon creation
        /// </summary>
        private IDictionary<string, DBOutputTypeDesc> _outputParameters;

        /// <summary>
        /// Command text that needs to be associated with the command.
        /// </summary>
        private readonly string _dbCommandText;

        /// <summary>
        /// Command timeout
        /// </summary>
        private readonly int? _dbCommandTimeout;

        /// <summary>
        /// Column settings
        /// </summary>
        private readonly ColumnSettings _metadataSettings;

        /// <summary>
        /// Private lock for connection and command.
        /// </summary>
        private object _allocLock;

        /// <summary>
        /// Connection allocated to this instance
        /// </summary>
        private DbConnection _theConnection;

        /// <summary>
        /// Command allocated to this instance
        /// </summary>
        private DbCommand _theCommand;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseDbDriverCommand"/> class.
        /// </summary>
        /// <param name="driver">The driver.</param>
        /// <param name="fragments">The fragments.</param>
        /// <param name="inputParameters">The input parameters.</param>
        /// <param name="dbCommandText">The command text.</param>
        /// <param name="dbCommandTimeout">The db command timeout.</param>
        /// <param name="metadataSettings">The metadata settings.</param>
        protected internal BaseDbDriverCommand(
            BaseDbDriver driver,
            IEnumerable<PlaceholderParser.Fragment> fragments,
            IEnumerable<string> inputParameters,
            string dbCommandText,
            int? dbCommandTimeout,
            ColumnSettings metadataSettings)
        {
            _driver = driver;
            _metadataSettings = metadataSettings;
            _fragments = new List<PlaceholderParser.Fragment>(fragments);
            _inputParameters = new List<string>(inputParameters);
            _dbCommandText = dbCommandText;
            _dbCommandTimeout = dbCommandTimeout;

            _allocLock = new object();
            _theConnection = null;
            _theCommand = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseDbDriverCommand"/> class.
        /// Used for cloning.
        /// </summary>
        protected internal BaseDbDriverCommand()
        {
        }

        /// <summary>
        /// Clones the driver command.
        /// </summary>
        /// <returns></returns>
        public virtual DbDriverCommand Clone()
        {
            var dbClone = (BaseDbDriverCommand) MemberwiseClone();
            // Create an independent lock
            dbClone._allocLock = new object();
            // Ensure theConnection and theCommand are not copied
            dbClone._theConnection = null;
            dbClone._theCommand = null;
            // Return the clone
            return dbClone;
        }

        #region IDisposable Members

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            lock (_allocLock) {
                // Clean up the command
                if (_theCommand != null) {
                    _theCommand.Dispose();
                    _theCommand = null;
                }

                // Clean up the connection
                //if (theConnection != null)
                //{
                //    theConnection.Dispose();
                //    theConnection = null;
                //}
            }
        }

        #endregion

        private DbCommand CreateCommand(
            DbConnection dbConnection,
            bool honorTimeout)
        {
            DbCommand myCommand;
            // Create the command
            myCommand = dbConnection.CreateCommand();
            myCommand.CommandType = CommandType.Text;
            myCommand.CommandText = _dbCommandText;
            if (_dbCommandTimeout.HasValue && honorTimeout) {
                myCommand.CommandTimeout = _dbCommandTimeout.Value;
            }

            // Bind the parameters
            myCommand.Parameters.Clear();
            foreach (var parameterName in _inputParameters) {
                var myParam = myCommand.CreateParameter();
                myParam.IsNullable = true;
                myParam.ParameterName = parameterName;
                myParam.Value = DBNull.Value;
                myCommand.Parameters.Add(myParam);
            }

            return myCommand;
        }

        /// <summary>
        /// Ensures that the command is allocated.
        /// </summary>
        protected virtual void AllocateCommand()
        {
            lock (_allocLock) {
                if (_theCommand == null) {
                    // Create the connection
                    _theConnection = _driver.CreateConnectionInternal();
                    // Create the command
                    _theCommand = CreateCommand(_theConnection, true);
                }
            }
        }

        /// <summary>
        /// Gets the actual database command.
        /// </summary>
        /// <value>The command.</value>
        public virtual DbCommand Command {
            get {
                AllocateCommand();
                return _theCommand;
            }
        }

        /// <summary>
        /// Gets the parameters.
        /// </summary>
        /// <param name="parseFragements">The parse fragements.</param>
        /// <returns></returns>
        private static SQLParameterDesc GetParameters(IEnumerable<PlaceholderParser.Fragment> parseFragements)
        {
            var eventPropertyParams = new List<string>();
            var builtinParams = new List<string>();
            foreach (var fragment in parseFragements) {
                if (fragment.IsParameter) {
                    if (fragment.Value == SAMPLE_WHERECLAUSE_PLACEHOLDER) {
                        builtinParams.Add(fragment.Value);
                    }
                    else {
                        eventPropertyParams.Add(fragment.Value);
                    }
                }
            }

            IList<string> paramList = eventPropertyParams;
            IList<string> builtin = eventPropertyParams;
            return new SQLParameterDesc(paramList, builtin);
        }

        /// <summary>
        /// Gets the fragments.
        /// </summary>
        /// <value>The fragments.</value>
        public virtual IEnumerable<PlaceholderParser.Fragment> Fragments => _fragments;

        /// <summary>
        /// Gets the pseudo text.
        /// </summary>
        /// <value>The pseudo text.</value>
        public virtual string PseudoText {
            get {
                var builder = new StringBuilder();
                foreach (var fragment in Fragments) {
                    if (fragment.IsParameter) {
                        if (fragment.Value != SAMPLE_WHERECLAUSE_PLACEHOLDER) {
                            builder.Append('?');
                        }
                    }
                    else {
                        builder.Append(fragment.Value);
                    }
                }

                return builder.ToString();
            }
        }

        /// <summary>
        /// Gets the command text.
        /// </summary>
        /// <value>The command text.</value>
        public virtual string CommandText => _dbCommandText;

        #region DbDriverCommand Members

        /// <summary>
        /// Gets the driver associated with this command.
        /// </summary>
        /// <value></value>
        public virtual DbDriver Driver => _driver;

        /// <summary>
        /// Gets the meta data.
        /// </summary>
        /// <value>The meta data.</value>
        public virtual QueryMetaData MetaData => new QueryMetaData(InputParameters, OutputParameters);

        /// <summary>
        /// Gets the meta data settings associated with this command.
        /// </summary>
        public ColumnSettings MetaDataSettings => _metadataSettings;

        /// <summary>
        /// Gets a list of parameters.
        /// </summary>
        /// <value>The parameters.</value>
        public virtual SQLParameterDesc ParameterDescription => GetParameters(_fragments);

        /// <summary>
        /// Gets the input parameters.
        /// </summary>
        /// <value>The input parameters.</value>
        public virtual IList<string> InputParameters => _inputParameters;

        /// <summary>
        /// Gets the output parameters.
        /// </summary>
        /// <value>The output parameters.</value>
        public virtual IDictionary<string, DBOutputTypeDesc> OutputParameters {
            get {
                if (_outputParameters == null) {
                    CreateOutputParameters();
                }

                return _outputParameters;
            }

            protected set => _outputParameters = value;
        }

        /// <summary>
        /// Creates and sets the output parameters
        /// </summary>
        protected virtual void CreateOutputParameters()
        {
            try {
                if (Log.IsInfoEnabled) {
                    Log.Info(".OutputParameters - dbCommandText = '" + _dbCommandText + "'");
                }

                // This embodies the default behavior of the BaseDbDriver and how it
                // handles the analysis of a query and the schema that is associated
                // with it.  If this handling is incorrect, you can (a) subclass and
                // provide your implementation or (b) submit the points you need
                // interceptors to be added so that we can provide you with the
                // right hooks.  ADO.NET can often be difficult to navigate.

                DataTable schemaTable;

                var dbConnection = _driver.CreateConnectionInternal();

                using (var dbCommand = CreateCommand(dbConnection, false)) {
                    try {
                        using (IDataReader reader = dbCommand.ExecuteReader(CommandBehavior.SchemaOnly)) {
                            // Get the schema table
                            schemaTable = reader.GetSchemaTable();
                        }
                    }
                    catch (DbException ex) {
                        var text = "Error in statement '" +
                                   _dbCommandText +
                                   "', failed to obtain result metadata, consider turning off metadata interrogation via configuration";
                        Log.Error(text, ex);
                        throw new ExprValidationException(
                            text + ", please check the statement, reason: " + ex.Message,
                            ex);
                    }

                    if (Log.IsDebugEnabled) {
                        Log.Debug(".OutputParameters value = " + _outputParameters);
                    }
                }

                // Analyze the schemaTable
                _outputParameters = CompileSchemaTable(schemaTable, _metadataSettings);
            }
            catch (DbException ex) {
                var text = "Error preparing statement '" + _dbCommandText + '\'';
                Log.Error(text, ex);
                throw new ExprValidationException(text + ", reason: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Gets the type of the column associated with the row in the
        /// table schema.
        /// </summary>
        /// <param name="schemaDataRow">The schema data row.</param>
        /// <returns></returns>
        protected virtual Type GetColumnType(DataRow schemaDataRow)
        {
            var columnType = (Type) schemaDataRow["DataType"];
            var columnSize = (int) schemaDataRow["ColumnSize"];

            // Some providers (read MySQL) provide bools as an integer
            // with a size of 1.  We should probably convert these to bool
            // to make client integration easier.
            if ((columnType == typeof(sbyte)) && (columnSize == 1)) {
                columnType = typeof(bool);
            }

            return columnType;
        }

        /// <summary>
        /// Gets the SQL type of the column associated with the row in the
        /// table schema.
        /// </summary>
        /// <param name="schemaDataRow">The schema data row.</param>
        /// <returns></returns>
        protected virtual string GetColumnSqlType(DataRow schemaDataRow)
        {
            var dataType = (Type) schemaDataRow["DataType"];
            return dataType.FullName;

            //var providerType = (Int32)schemaDataRow["ProviderType"];
            //var providerTypeAsEnum = (DbType) providerType;
            //var sqlTypeName = EnumHelper.GetName(providerTypeAsEnum);
            //return sqlTypeName;
        }

        /// <summary>
        /// Compiles the schema table.
        /// </summary>
        /// <param name="schemaTable">The schema table.</param>
        /// <param name="columnSettings">The column settings.</param>
        /// <returns></returns>
        protected virtual IDictionary<string, DBOutputTypeDesc> CompileSchemaTable(
            DataTable schemaTable,
            ColumnSettings columnSettings)
        {
            IDictionary<string, DBOutputTypeDesc> outputProperties = new Dictionary<string, DBOutputTypeDesc>();
            foreach (DataRow dataRow in schemaTable.Rows) {
                var columnName = (string) dataRow["ColumnName"];
                var columnType = GetColumnType(dataRow);
                var sqlTypeName = GetColumnSqlType(dataRow);
                //var canBeNull = (Boolean)dataRow["AllowDBNull"];
                //if (canBeNull) {
                //    columnType = columnType.GetBoxedType();
                //}

                // Address column case management
                var caseEnum = columnSettings.ColumnCaseConversionEnum;
                switch (caseEnum) {
                    case ColumnChangeCaseEnum.LOWERCASE:
                        columnName = columnName.ToLower();
                        break;

                    case ColumnChangeCaseEnum.UPPERCASE:
                        columnName = columnName.ToUpper();
                        break;
                }

                // Setup type binding
                DatabaseTypeBinding binding = null;

                // Check the typeBinding; the typeBinding tells us if we are
                // converting the resultant type from the dataType that has been
                // provided to us into a different type.

                //if (columnSettings.SqlTypeBinding != null)
                //{
                //    String typeBinding = columnSettings.SqlTypeBinding.Get(columnType);
                //    if (typeBinding != null)
                //    {
                //        binding = DatabaseTypeEnum.GetEnum(typeBinding).Binding;
                //    }
                //}

                var outputDesc = new DBOutputTypeDesc(sqlTypeName, columnType, binding);
                outputProperties[columnName] = outputDesc;
            }

            return outputProperties;
        }

        #endregion

        private static readonly ILog Log =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }

    /// <summary>
    /// Creates database command objects
    /// </summary>
    /// <returns></returns>
    public delegate IDbCommand DbCommandFactory();
}