///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.epl.agg.aggregator
{
    /// <summary>Aggregator for the very last value.</summary>
    public class AggregatorLeavingFilter : AggregatorLeaving
    {
        public override void Leave(object parameters)
        {
            var pass = parameters;
            if (true.Equals(parameters)) base.Leave(null);
        }
    }
} // end of namespace