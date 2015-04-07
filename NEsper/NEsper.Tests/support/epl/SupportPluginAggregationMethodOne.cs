///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.epl.agg.aggregator;

namespace com.espertech.esper.support.epl
{
    [Serializable]
    public class SupportPluginAggregationMethodOne : AggregationMethod
    {
        private int _count;
    
        public void Clear()
        {
            _count = 0;    
        }

        public void Enter(Object value)
        {
            _count--;
        }
    
        public void Leave(Object value)
        {
            _count++;
        }

        public object Value
        {
            get { return _count; }
        }

        public Type ValueType
        {
            get { return typeof(int?); }
        }
    }
}
