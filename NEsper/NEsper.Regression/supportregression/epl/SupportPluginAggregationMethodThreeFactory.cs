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

namespace com.espertech.esper.supportregression.epl
{
    [Serializable]
    public class SupportPluginAggregationMethodThreeFactory : AggregationFunctionFactory
    {
        private static IList<AggregationValidationContext> contexts = new List<AggregationValidationContext>();

        public static IList<AggregationValidationContext> Contexts
        {
            get { return contexts; }
        }

        public void Validate(AggregationValidationContext validationContext)
        {
            contexts.Add(validationContext);
        }

        public Type ValueType
        {
            get { return typeof (int); }
        }

        public string FunctionName
        {
            set { }
        }

        public AggregationMethod NewAggregator()
        {
            return new SupportPluginAggregationMethodThree();
        }
    }
}
