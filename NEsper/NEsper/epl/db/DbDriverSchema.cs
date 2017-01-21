///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System.Collections.Generic;

namespace com.espertech.esper.epl.db
{
    /// <summary>
    /// Provides the schema associated with a command.
    /// </summary>

    public interface DbDriverSchema
    {
        /// <summary>
        /// Gets the column names.
        /// </summary>
        /// <value>The column names.</value>
        IEnumerable<string> ColumnNames { get; }

        /// <summary>
        /// Gets the column info.
        /// </summary>
        /// <value>The column info.</value>
        IEnumerable<KeyValuePair<string, DBOutputTypeDesc>> ColumnInfo { get; }

        /// <summary>
        /// Gets the <see cref="com.espertech.esper.epl.db.DBOutputTypeDesc"/> associated
        /// with the given column name.
        /// </summary>
        /// <value></value>
        DBOutputTypeDesc this[string index] { get; }
    }
}
