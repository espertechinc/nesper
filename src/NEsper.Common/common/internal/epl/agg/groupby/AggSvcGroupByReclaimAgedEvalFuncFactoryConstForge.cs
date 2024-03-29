///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.agg.groupby
{
    public class AggSvcGroupByReclaimAgedEvalFuncFactoryConstForge : AggSvcGroupByReclaimAgedEvalFuncFactoryForge
    {
        private readonly double valueDouble;

        public AggSvcGroupByReclaimAgedEvalFuncFactoryConstForge(double valueDouble)
        {
            this.valueDouble = valueDouble;
        }

        public CodegenExpressionInstanceField Make(CodegenClassScope classScope)
        {
            return classScope.AddDefaultFieldUnshared(
                true,
                typeof(AggSvcGroupByReclaimAgedEvalFuncFactoryConst),
                NewInstance<AggSvcGroupByReclaimAgedEvalFuncFactoryConst>(Constant(valueDouble)));
        }
    }
} // end of namespace