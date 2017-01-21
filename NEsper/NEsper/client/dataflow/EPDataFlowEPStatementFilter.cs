///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.client.dataflow
{
    /// <summary>Filter for use with <seealso cref="com.espertech.esper.dataflow.ops.EPStatementSource" /> operator. </summary>
    public interface EPDataFlowEPStatementFilter
    {
        /// <summary>Pass or skip the statement. </summary>
        /// <param name="statement">to test</param>
        /// <returns>indicator whether to include (true) or exclude (false) the statement.</returns>
        bool Pass(EPStatement statement);
    }
}
