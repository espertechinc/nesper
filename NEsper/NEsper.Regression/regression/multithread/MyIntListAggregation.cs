///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;

using com.espertech.esper.epl.agg.aggregator;

namespace com.espertech.esper.regression.multithread
{
    public class MyIntListAggregation : AggregationMethod
    {
        private readonly List<int> _values = new List<int>();
    
        public void Enter(Object value) {
            _values.Add((int) value);
        }
    
        public void Leave(Object value) {
        }

        public object Value
        {
            get { return new List<int>(_values); }
        }

        public void Clear() {
            _values.Clear();
        }
    }
}
