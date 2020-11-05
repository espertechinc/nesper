///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat;

namespace com.espertech.esper.common.client.hook.type
{
    /// <summary>
    ///     For use with <seealso cref="SQLColumnTypeConversion" />, context of column conversion.
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
        public SQLColumnTypeContext(
            string db,
            string sql,
            string columnName,
            Type columnClassType,
            string columnSqlType,
            int columnNumber)
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
        public string Db { get; }

        /// <summary>Returns sql. </summary>
        /// <returns>sql</returns>
        public string Sql { get; }

        /// <summary>Returns column name. </summary>
        /// <returns>name</returns>
        public string ColumnName { get; }

        /// <summary>Returns column type. </summary>
        /// <returns>column type</returns>
        public Type ColumnClassType { get; }

        /// <summary>Returns column sql type. </summary>
        /// <returns>sql type</returns>
        public string ColumnSqlType { get; }

        /// <summary>Returns column number starting at 1. </summary>
        /// <returns>column number</returns>
        public int ColumnNumber { get; }
    }
}