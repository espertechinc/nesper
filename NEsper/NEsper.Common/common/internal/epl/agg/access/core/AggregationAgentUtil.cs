///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.hook.aggmultifunc;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.agg.access.core
{
    public class AggregationAgentUtil
    {
        public static CodegenExpression MakeArray(
            AggregationAgentForge[] accessAgents,
            CodegenMethod method,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            CodegenExpression[] inits = new CodegenExpression[accessAgents.Length];
            for (int i = 0; i < inits.Length; i++) {
                inits[i] = accessAgents[i].Make(method, symbols, classScope);
            }

            return NewArrayWithInit(typeof(AggregationMultiFunctionAgent), inits);
        }
    }
} // end of namespace