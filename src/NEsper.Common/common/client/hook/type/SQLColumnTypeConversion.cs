///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.client.hook.type
{
    /// <summary>
    /// Implement this interface when providing a callback for SQL input parameter and column result processing for a 
    /// statement, converting an input parameter or converting an output column value into any other value.
    /// <para/>
    /// An instance of the class implementating this interface exists typically per statement that the callback has
    /// been registered for by means of EPL statement annotation.
    /// </summary>
    public interface SQLColumnTypeConversion
    {
        /// <summary>
        /// Return the new type of the column. To leave the type unchanged, return 
        /// <seealso cref="SQLColumnTypeContext.ColumnClassType" /> or null.
        /// </summary>
        /// <param name="context">contains the database name, query fired, column name, column type and column number</param>
        /// <returns>type of column after conversion</returns>
        Type GetColumnType(SQLColumnTypeContext context);

        /// <summary>
        /// Return the new value of the column. To leave the value unchanged, 
        /// return <seealso cref="SQLColumnValueContext.ColumnValue" />.
        /// </summary>
        /// <param name="context">contains the column name, column value and column number</param>
        /// <returns>value of column after conversion</returns>
        object GetColumnValue(SQLColumnValueContext context);

        /// <summary>
        /// Return the new value of the input parameter. To leave the value unchanged, 
        /// return <seealso cref="SQLInputParameterContext.ParameterValue" />.
        /// </summary>
        /// <param name="context">contains the parameter name and number</param>
        /// <returns>value of parameter after conversion</returns>
        object GetParameterValue(SQLInputParameterContext context);
    }
}