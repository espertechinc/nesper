using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.agg.core
{
    public static class AggregationCodegenUpdateTypeExtensions
    {
        public static IList<CodegenNamedParam> GetParams(this AggregationCodegenUpdateType value)
        {
            switch (value) {
                case AggregationCodegenUpdateType.APPLYENTER:
                    return AggregationServiceFactoryCompiler.UPDPARAMS;

                case AggregationCodegenUpdateType.APPLYLEAVE:
                    return AggregationServiceFactoryCompiler.UPDPARAMS;

                case AggregationCodegenUpdateType.CLEAR:
                    return EmptyList<CodegenNamedParam>.Instance;

                default:
                    throw new ArgumentOutOfRangeException(nameof(value));
            }
        }
    }
}