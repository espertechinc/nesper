///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

namespace com.espertech.esper.common.client.dataflow.core
{
    /// <summary>Interface for statictics of data flow instance. </summary>
    public interface EPDataFlowInstanceStatistics
    {
        /// <summary>Returns operator stats. </summary>
        /// <value>stats</value>
        IList<EPDataFlowInstanceOperatorStat> OperatorStatistics { get; }
    }
}
