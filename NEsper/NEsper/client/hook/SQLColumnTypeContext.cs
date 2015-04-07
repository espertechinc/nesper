///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.util;

namespace com.espertech.esper.client.hook
{
    /// <summary>
    /// For use with <seealso cref="SQLColumnTypeConversion" />, context of column conversion.
    /// </summary>
    public class SQLColumnTypeContext
    {
        /// <summary>Ctor. </summary>
        /// <param name="db">database</param>
        /// <param name="sql">sql</param>
        /// <param name="columnName">column name</param>
        /// <param name="columnClassType">column type</param>
        /// <param name="columnSqlType">sql type</param>
        /// <param name="columnNumber">column number starting at 1</param>
        public SQLColumnTypeContext(String db, String sql, String columnName, Type columnClassType, string columnSqlType, int columnNumber)
        {
            Db = db;
            Sql = sql;
            ColumnName = columnName;
            ColumnClassType = columnClassType.GetBoxedType();
            ColumnSqlType = columnSqlType;
            ColumnNumber = columnNumber;
        }

        /// <summary>Get database name. </summary>
        /// <returns>db name</returns>
        public string Db { get; private set; }

        /// <summary>Returns sql. </summary>
        /// <returns>sql</returns>
        public string Sql { get; private set; }

        /// <summary>Returns column name. </summary>
        /// <returns>name</returns>
        public string ColumnName { get; private set; }

        /// <summary>Returns column type. </summary>
        /// <returns>column type</returns>
        public Type ColumnClassType { get; private set; }

        /// <summary>Returns column sql type. </summary>
        /// <returns>sql type</returns>
        public string ColumnSqlType { get; private set; }

        /// <summary>Returns column number starting at 1. </summary>
        /// <returns>column number</returns>
        public int ColumnNumber { get; private set; }
    }
}
