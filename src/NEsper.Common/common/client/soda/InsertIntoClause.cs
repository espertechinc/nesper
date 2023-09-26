///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;

using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.client.soda
{
    /// <summary>
    /// An insert-into clause consists of a stream name and column names and an optional stream selector.
    /// </summary>
    [Serializable]
    public class InsertIntoClause
    {
        private StreamSelector streamSelector;
        private string streamName;
        private IList<string> columnNames;
        private Expression eventPrecedence;

        /// <summary>
        /// Ctor.
        /// </summary>
        public InsertIntoClause()
        {
        }

        /// <summary>
        /// Creates the insert-into clause.
        /// </summary>
        /// <param name = "streamName">the name of the stream to insert into</param>
        /// <returns>clause</returns>
        public static InsertIntoClause Create(string streamName)
        {
            return new InsertIntoClause(streamName);
        }

        /// <summary>
        /// Creates the insert-into clause.
        /// </summary>
        /// <param name = "streamName">the name of the stream to insert into</param>
        /// <param name = "columns">is a list of column names</param>
        /// <returns>clause</returns>
        public static InsertIntoClause Create(
            string streamName,
            params string[] columns)
        {
            return new InsertIntoClause(streamName, columns);
        }

        /// <summary>
        /// Creates the insert-into clause.
        /// </summary>
        /// <param name = "streamName">the name of the stream to insert into</param>
        /// <param name = "columns">is a list of column names</param>
        /// <param name = "streamSelector">selects the stream</param>
        /// <returns>clause</returns>
        public static InsertIntoClause Create(
            string streamName,
            string[] columns,
            StreamSelector streamSelector)
        {
            return Create(streamName, columns, streamSelector, null);
        }

        /// <summary>
        /// Creates the insert-into clause.
        /// </summary>
        /// <param name = "streamName">the name of the stream to insert into</param>
        /// <param name = "columns">is a list of column names</param>
        /// <param name = "streamSelector">selects the stream</param>
        /// <param name = "precedence">event precedence or null when not applicable</param>
        /// <returns>clause</returns>
        public static InsertIntoClause Create(
            string streamName,
            string[] columns,
            StreamSelector streamSelector,
            Expression precedence)
        {
            if (streamSelector == StreamSelector.RSTREAM_ISTREAM_BOTH) {
                throw new ArgumentException("Insert into only allows istream or rstream selection, not both");
            }

            return new InsertIntoClause(streamName, Arrays.AsList(columns), streamSelector, precedence);
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name = "streamName">is the stream name to insert into</param>
        public InsertIntoClause(string streamName)
        {
            streamSelector = StreamSelector.ISTREAM_ONLY;
            this.streamName = streamName;
            columnNames = new List<string>();
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name = "streamName">is the stream name to insert into</param>
        /// <param name = "columnNames">column names</param>
        public InsertIntoClause(
            string streamName,
            string[] columnNames)
        {
            streamSelector = StreamSelector.ISTREAM_ONLY;
            this.streamName = streamName;
            this.columnNames = Arrays.AsList(columnNames);
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name = "streamName">is the stream name to insert into</param>
        /// <param name = "columnNames">column names</param>
        /// <param name = "streamSelector">selector for either insert stream (the default) or remove stream or both</param>
        public InsertIntoClause(
            string streamName,
            IList<string> columnNames,
            StreamSelector streamSelector)
            : this(streamName, columnNames, streamSelector, null)
        {
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name = "streamName">is the stream name to insert into</param>
        /// <param name = "columnNames">column names</param>
        /// <param name = "streamSelector">selector for either insert stream (the default) or remove stream or both</param>
        /// <param name = "eventPrecedence">event precedence or null if none provided</param>
        public InsertIntoClause(
            string streamName,
            IList<string> columnNames,
            StreamSelector streamSelector,
            Expression eventPrecedence)
        {
            this.streamSelector = streamSelector;
            this.streamName = streamName;
            this.columnNames = columnNames;
            this.eventPrecedence = eventPrecedence;
        }

        /// <summary>
        /// Add a column name to the insert-into clause.
        /// </summary>
        /// <param name = "columnName">to add</param>
        public void Add(string columnName)
        {
            columnNames.Add(columnName);
        }

        /// <summary>
        /// Renders the clause in textual representation.
        /// </summary>
        /// <param name = "writer">to output to</param>
        /// <param name = "formatter">for newline-whitespace formatting</param>
        /// <param name = "isTopLevel">to indicate if this insert-into-clause is inside other clauses.</param>
        public void ToEPL(
            TextWriter writer,
            EPStatementFormatter formatter,
            bool isTopLevel)
        {
            formatter.BeginInsertInto(writer, isTopLevel);
            writer.Write("insert ");
            if (streamSelector != StreamSelector.ISTREAM_ONLY) {
                writer.Write(streamSelector.GetEPL());
                writer.Write(" ");
            }

            writer.Write("into ");
            writer.Write(streamName);
            if (columnNames.Count > 0) {
                writer.Write("(");
                var delimiter = "";
                foreach (var name in columnNames) {
                    writer.Write(delimiter);
                    writer.Write(name);
                    delimiter = ", ";
                }

                writer.Write(")");
            }

            writer.Write(" ");
            if (eventPrecedence != null) {
                writer.Write("event-precedence(");
                eventPrecedence.ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
                writer.Write(") ");
            }
        }

        public StreamSelector StreamSelector {
            get => streamSelector;

            set => streamSelector = value;
        }

        public string StreamName {
            get => streamName;

            set => streamName = value;
        }

        public IList<string> ColumnNames {
            get => columnNames;

            set => columnNames = value;
        }

        public Expression EventPrecedence {
            get => eventPrecedence;

            set => eventPrecedence = value;
        }
    }
} // end of namespace