///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Data;

namespace com.espertech.esper.client.hook
{
    /// <summary>
    /// For use with <see cref="SQLColumnTypeConversion" />, context of column conversion. 
    /// Contains the columns information as well as the column result value after reading 
    /// the value and the result set itself for direct access, if required.
    /// <para/>
    /// Applications should not retain instances of this class as the engine may change 
    /// and reuse values here.
    /// </summary>
    public class SQLColumnValueContext
    {
        /// <summary>
        /// Returns column name.
        /// </summary>
        /// <value>The name of the column.</value>
        /// <returns>name</returns>
        public string ColumnName { get; set; }

        /// <summary>
        /// Returns column number.
        /// </summary>
        /// <value>The column number.</value>
        /// <returns>column number</returns>
        public int ColumnNumber { get; set; }

        /// <summary>
        /// Returns column value
        /// </summary>
        /// <value>The column value.</value>
        /// <returns>value</returns>
        public object ColumnValue { get; set; }

        /// <summary>
        /// Returns the result set.
        /// </summary>
        /// <value>result set</value>
        public IDataReader ResultSet { get; set; }
    }
}
