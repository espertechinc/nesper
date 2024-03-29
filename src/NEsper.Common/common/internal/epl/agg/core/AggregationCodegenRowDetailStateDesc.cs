///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.epl.agg.core
{
    public class AggregationCodegenRowDetailStateDesc
    {
        public AggregationCodegenRowDetailStateDesc(
            ExprForge[][] optionalMethodForges,
            AggregationForgeFactory[] methodFactories,
            AggregationStateFactoryForge[] accessStateForges)
        {
            OptionalMethodForges = optionalMethodForges;
            MethodFactories = methodFactories;
            AccessStateForges = accessStateForges;
        }

        public ExprForge[][] OptionalMethodForges { get; }

        public AggregationForgeFactory[] MethodFactories { get; }

        public AggregationStateFactoryForge[] AccessStateForges { get; }
    }
} // end of namespace