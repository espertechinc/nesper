///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.compile.stage3;

namespace com.espertech.esper.compiler.client.option
{
    /// <summary>
    ///     Provides the environment to <seealso cref="AccessModifierTableOption" />.
    /// </summary>
    public class AccessModifierTableContext : StatementOptionContextBase
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="base">statement info</param>
        /// <param name="tableName">table name</param>
        public AccessModifierTableContext(
            StatementBaseInfo @base,
            string tableName)
            : base(@base)
        {
            TableName = tableName;
        }

        /// <summary>
        ///     Returns the table name
        /// </summary>
        /// <returns>table name</returns>
        public string TableName { get; }
    }
} // end of namespace