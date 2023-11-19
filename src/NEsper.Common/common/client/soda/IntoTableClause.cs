///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

namespace com.espertech.esper.common.client.soda
{
    /// <summary>
    /// Into-table clause.
    /// </summary>
    public class IntoTableClause
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="tableName">table name</param>
        public IntoTableClause(string tableName)
        {
            TableName = tableName;
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        public IntoTableClause()
        {
        }

        /// <summary>
        /// Returns the table name.
        /// </summary>
        /// <value>table name</value>
        public string TableName { get; set; }

        /// <summary>
        /// Renders the clause.
        /// </summary>
        /// <param name="writer">to write to</param>
        /// <param name="formatter">the formatter</param>
        public void ToEPL(
            TextWriter writer,
            EPStatementFormatter formatter)
        {
            formatter.BeginIntoTable(writer, true);
            writer.Write("into table ");
            writer.Write(TableName);
            writer.Write(" ");
        }
    }
}