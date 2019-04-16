///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.common.client.util;

namespace com.espertech.esper.common.@internal.context.util
{
    /// <summary>
    /// Factory for the managed lock that provides statement resource protection.
    /// </summary>
    public interface StatementAgentInstanceLockFactory
    {
        /// <summary>
        /// Create lock for statement
        /// </summary>
        /// <param name="statementName">is the statement name</param>
        /// <param name="annotations">annotation</param>
        /// <param name="stateless">indicator whether stateless</param>
        /// <param name="statementType">statement type</param>
        /// <returns>lock</returns>
        StatementAgentInstanceLock GetStatementLock(
            string statementName,
            Attribute[] annotations,
            bool stateless,
            StatementType statementType);
    }
} // end of namespace