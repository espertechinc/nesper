///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client.hook;
using com.espertech.esper.compat;
using com.espertech.esper.epl.agg.aggregator;
using com.espertech.esper.epl.agg.service;

namespace com.espertech.esper.supportregression.epl
{
    [Serializable]
    public class SupportPluginAggregationMethodTwoFactory : AggregationFunctionFactory
    {
        public void Validate(AggregationValidationContext validationContext)
        {
            throw new ArgumentException(string.Format(
                "Invalid parameter type '{0}', expecting string", 
                Name.Clean(validationContext.ParameterTypes[0])));
        }

        public string FunctionName
        {
            set { }
        }

        public AggregationMethod NewAggregator()
        {
            return new SupportPluginAggregationMethodTwo();
        }

        public Type ValueType
        {
            get { return null; }
        }
    }
}
