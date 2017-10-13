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

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.client.soda
{
    /// <summary>
    /// Represents a create-variable syntax for creating a new variable.
    /// </summary>
    [Serializable]
    public class CreateTableClause
    {
        private string _tableName;
        private IList<CreateTableColumn> _columns;
    
        /// <summary>
        /// Ctor.
        /// </summary>
        public CreateTableClause() {
        }
    
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="tableName">the table name</param>
        public CreateTableClause(string tableName) {
            this._tableName = tableName;
        }
    
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="tableName">the table name</param>
        /// <param name="columns">table columns</param>
        public CreateTableClause(string tableName, IList<CreateTableColumn> columns) {
            this._tableName = tableName;
            this._columns = columns;
        }

        /// <summary>
        /// Returns the table name
        /// </summary>
        /// <value>table name</value>
        public string TableName
        {
            get { return _tableName; }
            set { this._tableName = value; }
        }

        /// <summary>
        /// Returns the table columns
        /// </summary>
        /// <value>table columns</value>
        public IList<CreateTableColumn> Columns
        {
            get { return _columns; }
            set { this._columns = value; }
        }

        /// <summary>
        /// RenderAny create-table clause
        /// </summary>
        /// <param name="writer">to render to</param>
        public void ToEPL(TextWriter writer)
        {
            writer.Write("create table ");
            writer.Write(_tableName);
            writer.Write(" (");
            string delimiter = "";
            foreach (CreateTableColumn col in _columns) {
                writer.Write(delimiter);
                col.ToEPL(writer);
                delimiter = ", ";
            }
            writer.Write(")");
        }
    }
}
