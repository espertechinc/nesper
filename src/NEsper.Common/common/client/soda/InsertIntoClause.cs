///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;

namespace com.espertech.esper.common.client.soda
{
    /// <summary>
    /// An insert-into clause consists of a stream name and column names and an optional stream selector.
    /// </summary>
    [Serializable]
    public class InsertIntoClause
    {
        /// <summary>Ctor. </summary>
        public InsertIntoClause()
        {
        }

        /// <summary>Creates the insert-into clause. </summary>
        /// <param name="streamName">the name of the stream to insert into</param>
        /// <returns>clause</returns>
        public static InsertIntoClause Create(string streamName)
        {
            return new InsertIntoClause(streamName);
        }

        /// <summary>Creates the insert-into clause. </summary>
        /// <param name="streamName">the name of the stream to insert into</param>
        /// <param name="columns">is a list of column names</param>
        /// <returns>clause</returns>
        public static InsertIntoClause Create(
            string streamName,
            params string[] columns)
        {
            return new InsertIntoClause(streamName, columns);
        }

        /// <summary>Creates the insert-into clause. </summary>
        /// <param name="streamName">the name of the stream to insert into</param>
        /// <param name="columns">is a list of column names</param>
        /// <param name="streamSelector">selects the stream</param>
        /// <returns>clause</returns>
        public static InsertIntoClause Create(
            string streamName,
            string[] columns,
            StreamSelector streamSelector)
        {
            if (streamSelector == StreamSelector.RSTREAM_ISTREAM_BOTH)
            {
                throw new ArgumentException("Insert into only allows istream or rstream selection, not both");
            }

            return new InsertIntoClause(streamName, columns, streamSelector);
        }

        /// <summary>Ctor. </summary>
        /// <param name="streamName">is the stream name to insert into</param>
        public InsertIntoClause(string streamName)
        {
            StreamSelector = StreamSelector.ISTREAM_ONLY;
            StreamName = streamName;
            ColumnNames = new List<string>();
        }

        /// <summary>Ctor. </summary>
        /// <param name="streamName">is the stream name to insert into</param>
        /// <param name="columnNames">column names</param>
        public InsertIntoClause(
            string streamName,
            string[] columnNames)
        {
            StreamSelector = StreamSelector.ISTREAM_ONLY;
            StreamName = streamName;
            ColumnNames = columnNames;
        }

        /// <summary>Ctor. </summary>
        /// <param name="streamName">is the stream name to insert into</param>
        /// <param name="columnNames">column names</param>
        /// <param name="streamSelector">selector for either insert stream (the default) or remove stream or both</param>
        public InsertIntoClause(
            string streamName,
            IList<string> columnNames,
            StreamSelector streamSelector)
        {
            StreamSelector = streamSelector;
            StreamName = streamName;
            ColumnNames = columnNames;
        }

        /// <summary>Returns the stream selector for the insert into. </summary>
        /// <value>stream selector</value>
        public StreamSelector StreamSelector { get; set; }

        /// <summary>Returns name of stream name to use for insert-into stream. </summary>
        /// <value>stream name</value>
        public string StreamName { get; set; }

        /// <summary>Returns a list of column names specified optionally in the insert-into clause, or empty if none specified. </summary>
        /// <value>column names or empty list if none supplied</value>
        public IList<string> ColumnNames { get; set; }

        /// <summary>Add a column name to the insert-into clause. </summary>
        /// <param name="columnName">to add</param>
        public void Add(string columnName)
        {
            ColumnNames.Add(columnName);
        }

        /// <summary>Renders the clause in textual representation. </summary>
        /// <param name="writer">to output to</param>
        /// <param name="formatter">for NewLine-whitespace formatting</param>
        /// <param name="isTopLevel">to indicate if this insert-into-clause is inside other clauses.</param>
        public void ToEPL(
            TextWriter writer,
            EPStatementFormatter formatter,
            bool isTopLevel)
        {
            formatter.BeginInsertInto(writer, isTopLevel);
            writer.Write("insert ");
            if (StreamSelector != StreamSelector.ISTREAM_ONLY)
            {
                writer.Write(StreamSelector.GetEPL());
                writer.Write(" ");
            }

            writer.Write("into ");
            writer.Write(StreamName);

            if (ColumnNames.Count > 0)
            {
                writer.Write("(");
                string delimiter = "";
                foreach (var name in ColumnNames)
                {
                    writer.Write(delimiter);
                    writer.Write(name);
                    delimiter = ", ";
                }

                writer.Write(")");
            }
        }
    }
}