///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;

using com.espertech.esper.epl.agg.aggregator;
using com.espertech.esper.epl.core;

namespace com.espertech.esper.support.epl
{
    public class SupportAggregator : AggregationMethod
    {
        private int _sum;
    
        public void Clear()
        {
            
        }
    
        public void Enter(Object value)
        {
            if (value != null)
            {
                _sum += (int) value;
            }
        }
    
        public void Leave(Object value)
        {
            if (value != null)
            {
                _sum -= (int)value;
            }
        }

        public object Value
        {
            get { return _sum; }
        }

        public Type ValueType
        {
            get { return typeof(int?); }
        }

        public AggregationMethod NewAggregator(MethodResolutionService methodResolutionService)
        {
            return new SupportAggregator();
        }

        public string FunctionName
        {
            get { return "supportagg"; }
        }
    }
}
