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
    /// Counts all datapoints including null values.
    /// </summary>
    public class AggregatorCountFilter : AggregationMethod
    {
        protected long NumDataPoints;
    
        public void Clear()
        {
            NumDataPoints = 0;
        }
    
        public void Enter(Object @object)
        {
            if (CheckPass(@object))
            {
                NumDataPoints++;
            }
        }
    
        public void Leave(Object @object)
        {
            if (CheckPass(@object))
            {
                if (NumDataPoints > 0) {
                    NumDataPoints--;
                }
            }
        }

        public object Value
        {
            get { return NumDataPoints; }
        }

        private bool CheckPass(Object @object)
        {
            if (@object == null)
                return false;
            return true.Equals(@object);
        }
    }
}
