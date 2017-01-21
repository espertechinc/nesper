///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.core.service;

namespace com.espertech.esper.epl.metric
{
    /// <summary>SPI for metrics activity. </summary>
    public interface MetricReportingServiceSPI : MetricReportingService
    {
        /// <summary>Add stmt result listener. </summary>
        /// <param name="listener">to add</param>
        void AddStatementResultListener(StatementResultListener listener);
    
        /// <summary>Remove stmt result listener. </summary>
        /// <param name="listener">to remove</param>
        void RemoveStatementResultListener(StatementResultListener listener);

        /// <summary>Returns output hooks. </summary>
        /// <value>hooks.</value>
        ICollection<StatementResultListener> StatementOutputHooks { get; }
    }
}
