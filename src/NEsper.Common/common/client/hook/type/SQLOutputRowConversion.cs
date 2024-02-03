///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.client.hook.type
{
    /// <summary>
    /// Implement this interface when providing a callback for SQL row result processing for a statement,
    /// converting each row's values into a PONO.
    /// <para/>
    /// Rows can also be skipped via this callback, determined by the implementation returning a null value 
    /// for a row.
    /// <para/>
    /// An instance of the class implementating this interface exists typically per statement that the 
    /// callback has been registered for by means of EPL statement annotation.
    /// </summary>
    public interface SQLOutputRowConversion
    {
        /// <summary>Return the PONO class that represents a row of the SQL query result. </summary>
        /// <param name="context">receives the context information such as database name, query fired and types returned by query</param>
        /// <returns>class that represents a result row</returns>
        Type GetOutputRowType(SQLOutputRowTypeContext context);

        /// <summary>Returns the PONO object that represents a row of the SQL query result, or null to indicate to skip this row. </summary>
        /// <param name="context">receives row result information</param>
        /// <returns>PONO or null value to skip the row</returns>
        object GetOutputRow(SQLOutputRowValueContext context);
    }
}