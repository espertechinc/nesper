///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client.hook;
using com.espertech.esper.epl.agg.aggregator;
using com.espertech.esper.epl.agg.service;

namespace com.espertech.esper.multithread
{
    public class MyIntListAggregationFactory : AggregationFunctionFactory
    {
        public void Validate(AggregationValidationContext validationContext)
        {
        }

        public Type ValueType
        {
            get { return typeof (IList<int>); }
        }

        public string FunctionName
        {
            set { }
        }

        public AggregationMethod NewAggregator()
        {
            return new MyIntListAggregation();
        }
    }
}
