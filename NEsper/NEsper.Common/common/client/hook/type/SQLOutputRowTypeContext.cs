///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

namespace com.espertech.esper.common.client.hook.type
{
    /// <summary>
    /// For use with <seealso cref="SQLOutputRowConversion" />, context of row conversion.
    /// </summary>
    public class SQLOutputRowTypeContext
    {
        /// <summary>Ctor. </summary>
        /// <param name="db">database</param>
        /// <param name="sql">sql</param>
        /// <param name="fields">columns and their types</param>
        public SQLOutputRowTypeContext(
            string db,
            string sql,
            IDictionary<string, object> fields)
        {
            Db = db;
            Sql = sql;
            Fields = fields;
        }

        /// <summary>Returns the database name. </summary>
        /// <returns>database name</returns>
        public string Db { get; private set; }

        /// <summary>Returns the sql. </summary>
        /// <returns>sql</returns>
        public string Sql { get; private set; }

        /// <summary>Returns the column names and types. </summary>
        /// <returns>columns</returns>
        public IDictionary<string, object> Fields { get; private set; }
    }
}