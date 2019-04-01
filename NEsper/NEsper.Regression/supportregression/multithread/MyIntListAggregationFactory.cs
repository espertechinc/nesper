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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.agg.aggregator;
using com.espertech.esper.epl.agg.service;

using NUnit.Framework;

namespace com.espertech.esper.supportregression.multithread
{
    public class MyIntListAggregationFactory : AggregationFunctionFactory
    {
    
        public void Validate(AggregationValidationContext validationContext) {
        }

        public virtual Type ValueType
        {
            get { return typeof(List); }
        }

        public string FunctionName
        {
            set { }
        }

        public AggregationMethod NewAggregator() {
            return new MyIntListAggregation();
        }
    }
} // end of namespace
