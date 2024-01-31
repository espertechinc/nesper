///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.runtime.client;

using NEsper.Examples.Support;

namespace NEsper.Examples.MarketDataFeed
{
    public class TicksPerSecondStatement
    {
        public static EPStatement Create(EPRuntime runtime)
        {
            var stmt = "insert into TicksPerSecond " +
                          "select Feed as feed, count(*) as cnt from MarketDataEvent.win:time_batch(1 sec) group by Feed";

            return runtime.DeployStatement(stmt);
        }
    }
}
