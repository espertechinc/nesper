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
    /// <summary>Create an index on a named window.</summary>
    [Serializable]
    public class CreateIndexColumn
    {
        /// <summary>Ctor.</summary>
        public CreateIndexColumn()
        {
            IndexColumnType = CreateIndexColumnType.HASH;
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="columnName">column name</param>
        public CreateIndexColumn(string columnName)
        {
            IndexColumnType = CreateIndexColumnType.HASH;
            ColumnName = columnName;
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="columnName">colum name</param>
        /// <param name="type">index type</param>
        public CreateIndexColumn(string columnName, CreateIndexColumnType type)
        {
            ColumnName = columnName;
            IndexColumnType = type;
        }

        /// <summary>
        /// Renders the clause in textual representation.
        /// </summary>
        /// <param name="writer">to output to</param>
        public void ToEPL(TextWriter writer)
        {
            writer.Write(ColumnName);
            if (IndexColumnType != CreateIndexColumnType.HASH)
            {
                writer.Write(' ');
                writer.Write(IndexColumnType.ToString().ToLowerInvariant());
            }
        }

        /// <summary>
        /// Returns the column name.
        /// </summary>
        /// <value>column name</value>
        public string ColumnName { get; set; }

        /// <summary>
        /// Returns the index type.
        /// </summary>
        /// <value>index type</value>
        public CreateIndexColumnType IndexColumnType { get; set; }
    }
} // end of namespace
