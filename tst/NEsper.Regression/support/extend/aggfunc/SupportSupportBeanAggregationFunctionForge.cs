///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client.hook.aggfunc;
using com.espertech.esper.common.client.hook.forgeinject;
using com.espertech.esper.common.@internal.support;

namespace com.espertech.esper.regressionlib.support.extend.aggfunc
{
    public class SupportSupportBeanAggregationFunctionForge : AggregationFunctionForge
    {
        public string FunctionName {
            set { }
        }

        public void Validate(AggregationFunctionValidationContext validationContext)
        {
        }

        public Type ValueType => typeof(SupportBean);

        public AggregationFunctionMode AggregationFunctionMode =>
            new AggregationFunctionModeManaged().SetInjectionStrategyAggregationFunctionFactory(
                new InjectionStrategyClassNewInstance(typeof(SupportSupportBeanAggregationFunctionFactory)));
    }
} // end of namespace