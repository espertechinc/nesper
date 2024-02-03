///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

using NEsper.Examples.StockTicker.eventbean;

namespace NEsper.Examples.StockTicker.monitor
{
    public class StockTickerResultListener
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public int Count => MatchEvents.Count;

        public IList<object> MatchEvents { get; } = new List<object>().AsSyncList();

        public void Emitted(LimitAlert @object)
        {
            log.Info(".Emitted Received emitted " + @object);
            MatchEvents.Add(@object);
        }

        public void ClearMatched()
        {
            MatchEvents.Clear();
        }
    }
}