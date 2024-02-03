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
    public class AggregationRowStateForgeDesc
    {
        public AggregationRowStateForgeDesc(
            AggregationForgeFactory[] optionalMethodFactories,
            ExprForge[][] methodForges,
            AggregationStateFactoryForge[] accessFactoriesForges,
            AggregationAccessorSlotPairForge[] accessAccessorsForges,
            AggregationUseFlags useFlags)
        {
            MethodForges = methodForges;
            OptionalMethodFactories = optionalMethodFactories;
            AccessAccessorsForges = accessAccessorsForges;
            AccessFactoriesForges = accessFactoriesForges;
            UseFlags = useFlags;
        }

        public ExprForge[][] MethodForges { get; }

        public AggregationForgeFactory[] OptionalMethodFactories { get; }

        public AggregationAccessorSlotPairForge[] AccessAccessorsForges { get; }

        public AggregationStateFactoryForge[] AccessFactoriesForges { get; }

        public AggregationUseFlags UseFlags { get; }

        public int NumMethods => MethodForges?.Length ?? 0;

        public int NumAccess => AccessAccessorsForges?.Length ?? 0;
    }
} // end of namespace