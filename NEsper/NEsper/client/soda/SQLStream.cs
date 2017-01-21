///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

namespace com.espertech.esper.client.soda
{
    /// <summary>
    /// An SQL stream that polls via SQL for events via join.
    /// </summary>
    [Serializable]
    public class SQLStream : Stream
    {
        private String _databaseName;
        private String _sqlWithSubsParams;
        private String _optionalMetadataSQL;

        /// <summary>Ctor. </summary>
        public SQLStream() {
        }
    
        /// <summary>Creates a new SQL-based stream. </summary>
        /// <param name="databaseName">is the database name to poll</param>
        /// <param name="sqlWithSubsParams">is the SQL to use</param>
        /// <returns>stream</returns>
        public static SQLStream Create(String databaseName, String sqlWithSubsParams)
        {
            return new SQLStream(databaseName, sqlWithSubsParams, null, null);
        }
    
        /// <summary>Creates a new SQL-based stream. </summary>
        /// <param name="databaseName">is the database name to poll</param>
        /// <param name="sqlWithSubsParams">is the SQL to use</param>
        /// <param name="optStreamName">is the as-name of the stream</param>
        /// <returns>stream</returns>
        public static SQLStream Create(String databaseName, String sqlWithSubsParams, String optStreamName)
        {
            return new SQLStream(databaseName, sqlWithSubsParams, optStreamName, null);
        }
    
        /// <summary>Creates a new SQL-based stream. </summary>
        /// <param name="databaseName">is the database name to poll</param>
        /// <param name="sqlWithSubsParams">is the SQL to use</param>
        /// <param name="optStreamName">is the as-name of the stream</param>
        /// <param name="optionalMetadataSQL">optional SQL delivering metadata of statement</param>
        /// <returns>stream</returns>
        public static SQLStream Create(String databaseName, String sqlWithSubsParams, String optStreamName, String optionalMetadataSQL)
        {
            return new SQLStream(databaseName, sqlWithSubsParams, optStreamName, optionalMetadataSQL);
        }
    
        /// <summary>Ctor. </summary>
        /// <param name="databaseName">is the database name to poll</param>
        /// <param name="sqlWithSubsParams">is the SQL to use</param>
        /// <param name="optStreamName">is the optional as-name of the stream, or null if unnamed</param>
        /// <param name="optionalMetadataSQL">optional SQL delivering metadata of statement</param>
        public SQLStream(String databaseName, String sqlWithSubsParams, String optStreamName, String optionalMetadataSQL)

                    : base(optStreamName)
        {
            _databaseName = databaseName;
            _sqlWithSubsParams = sqlWithSubsParams;
            _optionalMetadataSQL = optionalMetadataSQL;
        }

        /// <summary>Returns the database name. </summary>
        /// <value>database name</value>
        public string DatabaseName
        {
            get { return _databaseName; }
            set { this._databaseName = value; }
        }

        /// <summary>Returns the SQL with optional substitution parameters in the SQL. </summary>
        /// <value>SQL</value>
        public string SqlWithSubsParams
        {
            get { return _sqlWithSubsParams; }
            set { this._sqlWithSubsParams = value; }
        }


        /// <summary>Returns the metadata SQL if any. </summary>
        /// <value>metadata SQL</value>
        public string OptionalMetadataSQL
        {
            get { return _optionalMetadataSQL; }
            set { this._optionalMetadataSQL = value; }
        }

        public override void ToEPLStream(TextWriter writer, EPStatementFormatter formatter)
        {
            writer.Write("sql:");
            writer.Write(_databaseName);
            writer.Write("[\"");
            writer.Write(_sqlWithSubsParams);
            writer.Write("\"]");
        }

        public override void ToEPLStreamType(TextWriter writer)
        {
            writer.Write("sql:");
            writer.Write(_databaseName);
            writer.Write("[..]");
        }

        public override void ToEPLStreamOptions(TextWriter writer)
        {        
        }
    }
}
