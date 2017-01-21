///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.epl.agg.service;

namespace com.espertech.esper.epl.agg.aggregator
{
    /// <summary>
    /// Aggregator for the count-ever value.
    /// </summary>
    public class AggregatorCountEverFilter : AggregatorCountEver
    {
        public override void Enter(object parameters)
        {
            var paramArray = (object[]) parameters;
            if (!AggregatorUtil.CheckFilter(paramArray))
            {
                return;
            }
            base.Enter(paramArray[0]);
        }
    }
}
