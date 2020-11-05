///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Data;

namespace com.espertech.esper.common.client.hook.type
{
    /// <summary>
    /// For use with <see cref="SQLOutputRowConversion"/>, context of row conversion. Provides
    /// row number, column values after reading the row as well as the result set itself for direct
    /// access.
    /// <para/>
    /// Applications should not retain instances of this class as the engine may change and reuse
    /// values here.
    /// </summary>
    public class SQLOutputRowValueContext
    {
        /// <summary>Return row number, the number of the current output row. </summary>
        /// <returns>row number</returns>
        public int RowNum { get; set; }

        /// <summary>Returns column values. </summary>
        /// <returns>values for all columns</returns>
        public IDictionary<string, object> Values { get; set; }

        /// <summary>
        /// Returns the result set.
        /// </summary>
        /// <value>result set</value>
        public IDataReader ResultSet { get; set; }
    }
}