///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.compat.threading;

namespace com.espertech.esper.core.service
{
    /// <summary>
    /// Factory for the managed lock that provides statement resource protection.
    /// </summary>
    public interface StatementLockFactory
    {
        /// <summary>
        /// Create lock for statement
        /// </summary>
        /// <param name="statementName">is the statement name</param>
        /// <param name="annotations">The annotations.</param>
        /// <param name="stateless">if set to <c>true</c> [stateless].</param>
        /// <returns>lock</returns>
        IReaderWriterLock GetStatementLock(string statementName, Attribute[] annotations, bool stateless);
    }
}
