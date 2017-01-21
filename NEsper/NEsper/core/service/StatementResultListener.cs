///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.client;

namespace com.espertech.esper.core.service
{
    /// <summary>
    /// Interface for statement result callbacks.
    /// </summary>
    public interface StatementResultListener
    {
        /// <summary>Provide statement result. </summary>
        /// <param name="newEvents">insert stream</param>
        /// <param name="oldEvents">remove stream</param>
        /// <param name="statementName">stmt name</param>
        /// <param name="statement">stmt</param>
        /// <param name="epServiceProvider">engine</param>
        void Update(EventBean[] newEvents,
                    EventBean[] oldEvents,
                    String statementName,
                    EPStatementSPI statement,
                    EPServiceProviderSPI epServiceProvider);
    }
}