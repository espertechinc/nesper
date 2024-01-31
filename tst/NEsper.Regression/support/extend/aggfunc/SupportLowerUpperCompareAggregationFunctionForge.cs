///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client.hook.aggfunc;
using com.espertech.esper.common.client.hook.forgeinject;

namespace com.espertech.esper.regressionlib.support.extend.aggfunc
{
    public class SupportLowerUpperCompareAggregationFunctionForge : AggregationFunctionForge
    {
        public static IList<AggregationFunctionValidationContext> Contexts { get; } =
            new List<AggregationFunctionValidationContext>();

        public void Validate(AggregationFunctionValidationContext validationContext)
        {
            Contexts.Add(validationContext);
        }

        public Type ValueType => typeof(int);

        public string FunctionName {
            set { }
        }

        public AggregationFunctionMode AggregationFunctionMode =>
            new AggregationFunctionModeMultiParam().SetInjectionStrategyAggregationFunctionFactory(
                new InjectionStrategyClassNewInstance(typeof(SupportLowerUpperCompareAggregationFunctionFactory)));
    }
} // end of namespace