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
    /// Represents a create-variable syntax for creating a new variable.
    /// </summary>
    [Serializable]
    public class CreateTableClause
    {
        private string tableName;
        private IList<CreateTableColumn> columns;

        /// <summary>
        /// Ctor.
        /// </summary>
        public CreateTableClause()
        {
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="tableName">the table name</param>
        public CreateTableClause(string tableName)
        {
            this.tableName = tableName;
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="tableName">the table name</param>
        /// <param name="columns">table columns</param>
        public CreateTableClause(
            string tableName,
            IList<CreateTableColumn> columns)
        {
            this.tableName = tableName;
            this.columns = columns;
        }

        /// <summary>
        /// Returns the table name
        /// </summary>
        /// <returns>table name</returns>
        public string TableName
        {
            get => tableName;
            set { tableName = value; }
        }

        /// <summary>
        /// Returns the table columns
        /// </summary>
        /// <returns>table columns</returns>
        public IList<CreateTableColumn> Columns
        {
            get => columns;
            set { columns = value; }
        }

        /// <summary>
        /// Render create-table clause
        /// </summary>
        /// <param name="writer">to render to</param>
        public virtual void ToEPL(TextWriter writer)
        {
            writer.Write("create table ");
            writer.Write(tableName);
            writer.Write(" (");
            string delimiter = "";
            foreach (CreateTableColumn col in columns)
            {
                writer.Write(delimiter);
                col.ToEPL(writer);
                delimiter = ", ";
            }

            writer.Write(")");
        }
    }
} // end of namespace