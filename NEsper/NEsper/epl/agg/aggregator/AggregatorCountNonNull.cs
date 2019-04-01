///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.epl.agg.aggregator
{
    /// <summary>
    /// Count all non-null values.
    /// </summary>
    public class AggregatorCountNonNull : AggregationMethod
    {
        protected long NumDataPoints;

        public AggregatorCountNonNull()
        {
        }

        public void Clear()
        {
            NumDataPoints = 0;
        }

        public void Enter(Object @object)
        {
            if (@object == null)
            {
                return;
            }
            NumDataPoints++;
        }

        public void Leave(Object @object)
        {
            if (@object == null)
            {
                return;
            }
            if (NumDataPoints > 0)
            {
                NumDataPoints--;
            }
        }

        public object Value
        {
            get { return NumDataPoints; }
        }
    }
}