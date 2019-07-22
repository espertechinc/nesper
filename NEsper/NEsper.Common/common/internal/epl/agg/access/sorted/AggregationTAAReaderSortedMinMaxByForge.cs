///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.agg.access.sorted
{
    public class AggregationTAAReaderSortedMinMaxByForge : AggregationTableAccessAggReaderForge
    {
        private readonly Type resultType;
        private readonly bool max;
        private readonly TableMetaData table;

        public AggregationTAAReaderSortedMinMaxByForge(
            Type resultType,
            bool max,
            TableMetaData table)
        {
            this.resultType = resultType;
            this.max = max;
            this.table = table;
        }

        public Type ResultType {
            get => resultType;
        }

        public CodegenExpression CodegenCreateReader(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            CodegenMethod method = parent.MakeChild(
                typeof(AggregationTAAReaderSortedMinMaxBy),
                this.GetType(),
                classScope);
            method.Block
                .DeclareVar<AggregationTAAReaderSortedMinMaxBy>(
                    "strat",
                    NewInstance(typeof(AggregationTAAReaderSortedMinMaxBy)))
                .SetProperty(Ref("strat"), "Max", Constant(max))
                .MethodReturn(@Ref("strat"));
            return LocalMethod(method);
        }
    }
} // end of namespace