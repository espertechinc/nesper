///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.IO;

namespace com.espertech.esper.common.client.soda
{
    /// <summary>
    /// An SQL stream that polls via SQL for events via join.
    /// </summary>
    public class SQLStream : Stream
    {
        private string databaseName;
        private string sqlWithSubsParams;
        private string optionalMetadataSQL;

        /// <summary>
        /// Ctor.
        /// </summary>
        public SQLStream()
        {
        }

        /// <summary>
        /// Creates a new SQL-based stream.
        /// </summary>
        /// <param name="databaseName">is the database name to poll</param>
        /// <param name="sqlWithSubsParams">is the SQL to use</param>
        /// <returns>stream</returns>
        public static SQLStream Create(
            string databaseName,
            string sqlWithSubsParams)
        {
            return new SQLStream(databaseName, sqlWithSubsParams, null, null);
        }

        /// <summary>
        /// Creates a new SQL-based stream.
        /// </summary>
        /// <param name="databaseName">is the database name to poll</param>
        /// <param name="sqlWithSubsParams">is the SQL to use</param>
        /// <param name="optStreamName">is the as-name of the stream</param>
        /// <returns>stream</returns>
        public static SQLStream Create(
            string databaseName,
            string sqlWithSubsParams,
            string optStreamName)
        {
            return new SQLStream(databaseName, sqlWithSubsParams, optStreamName, null);
        }

        /// <summary>
        /// Creates a new SQL-based stream.
        /// </summary>
        /// <param name="databaseName">is the database name to poll</param>
        /// <param name="sqlWithSubsParams">is the SQL to use</param>
        /// <param name="optStreamName">is the as-name of the stream</param>
        /// <param name="optionalMetadataSQL">optional SQL delivering metadata of statement</param>
        /// <returns>stream</returns>
        public static SQLStream Create(
            string databaseName,
            string sqlWithSubsParams,
            string optStreamName,
            string optionalMetadataSQL)
        {
            return new SQLStream(databaseName, sqlWithSubsParams, optStreamName, optionalMetadataSQL);
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="databaseName">is the database name to poll</param>
        /// <param name="sqlWithSubsParams">is the SQL to use</param>
        /// <param name="optStreamName">is the optional as-name of the stream, or null if unnamed</param>
        /// <param name="optionalMetadataSQL">optional SQL delivering metadata of statement</param>
        public SQLStream(
            string databaseName,
            string sqlWithSubsParams,
            string optStreamName,
            string optionalMetadataSQL)
            : base(optStreamName)
        {
            this.databaseName = databaseName;
            this.sqlWithSubsParams = sqlWithSubsParams;
            this.optionalMetadataSQL = optionalMetadataSQL;
        }

        /// <summary>
        /// Returns the database name.
        /// </summary>
        /// <returns>database name</returns>
        public string DatabaseName {
            get => databaseName;
            set => databaseName = value;
        }

        /// <summary>
        /// Sets the database name.
        /// </summary>
        /// <param name="databaseName">database name</param>
        public SQLStream SetDatabaseName(string databaseName)
        {
            this.databaseName = databaseName;
            return this;
        }

        /// <summary>
        /// Returns the SQL with optional substitution parameters in the SQL.
        /// </summary>
        /// <returns>SQL</returns>
        public string SqlWithSubsParams {
            get => sqlWithSubsParams;
            set => sqlWithSubsParams = value;
        }

        /// <summary>
        /// Sets the SQL with optional substitution parameters in the SQL.
        /// </summary>
        /// <param name="sqlWithSubsParams">SQL set set</param>
        public SQLStream SetSqlWithSubsParams(string sqlWithSubsParams)
        {
            this.sqlWithSubsParams = sqlWithSubsParams;
            return this;
        }

        /// <summary>
        /// Returns the metadata SQL if any.
        /// </summary>
        /// <returns>metadata SQL</returns>
        public string OptionalMetadataSQL
        {
            get => optionalMetadataSQL;
            set { this.optionalMetadataSQL = value; }
        }

        public override void ToEPLStream(
            TextWriter writer,
            EPStatementFormatter formatter)
        {
            writer.Write("sql:");
            writer.Write(databaseName);
            writer.Write("[\"");
            writer.Write(sqlWithSubsParams);
            writer.Write("\"]");
        }

        public override void ToEPLStreamType(TextWriter writer)
        {
            writer.Write("sql:");
            writer.Write(databaseName);
            writer.Write("[..]");
        }

        public override void ToEPLStreamOptions(TextWriter writer)
        {
        }
    }
} // end of namespace