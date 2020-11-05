///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.expression.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.variable.core
{
    public class VariableTriggerWriteCurlyForge : VariableTriggerWriteForge
    {
        private readonly ExprForge expression;
        private readonly string variableName;

        public VariableTriggerWriteCurlyForge(
            string variableName,
            ExprForge expression)
        {
            this.variableName = variableName;
            this.expression = expression;
        }

        public override CodegenExpression Make(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(VariableTriggerWriteCurly), GetType(), classScope);
            method.Block
                .DeclareVar<VariableTriggerWriteCurly>("desc", NewInstance(typeof(VariableTriggerWriteCurly)))
                .SetProperty(Ref("desc"), "VariableName", Constant(variableName))
                .SetProperty(Ref("desc"), "Expression",
                    ExprNodeUtilityCodegen.CodegenEvaluator(expression, method, typeof(VariableTriggerWriteCurlyForge), classScope))
                .MethodReturn(Ref("desc"));
            return LocalMethod(method);
        }
    }
} // end of namespace