///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.compat.function;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.metrics.instrumentation
{
    public static class InstrumentationCode
    {
        public static Consumer<CodegenBlock> Instblock(
            CodegenClassScope codegenClassScope,
            string name,
            params CodegenExpression[] expressions)
        {
            if (!codegenClassScope.IsInstrumented) {
                return block => { };
            }

            return block => Generate(block, name, expressions);
        }

        private static void Generate(
            CodegenBlock block,
            string name,
            params CodegenExpression[] expressions)
        {
            block.IfCondition(PublicConstValue(InstrumentationConstants.RUNTIME_HELPER_CLASS, "ENABLED"))
                .Expression(
                    ExprDotMethodChain(StaticMethod(InstrumentationConstants.RUNTIME_HELPER_CLASS, "get"))
                        .Add(name, expressions))
                .BlockEnd();
        }
    }
} // end of namespace