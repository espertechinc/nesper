///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.variable.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.context.aifactory.update
{
    public class InternalEventRouterWriterCurlyForge : InternalEventRouterWriterForge
    {
        private readonly ExprNode _expression;

        public InternalEventRouterWriterCurlyForge(ExprNode expression)
        {
            _expression = expression;
        }

        public override CodegenExpression Codegen(
            InternalEventRouterWriterForge writer,
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(InternalEventRouterWriterCurly), GetType(), classScope);
            var descExpr = ExprNodeUtilityCodegen.CodegenEvaluator(
                _expression.Forge,
                method,
                typeof(VariableTriggerWriteArrayElementForge),
                classScope);

            method.Block
                .DeclareVar<InternalEventRouterWriterCurly>("desc", NewInstance(typeof(InternalEventRouterWriterCurly)))
                .SetProperty(Ref("desc"), "Expression", descExpr)
                .MethodReturn(Ref("desc"));
            return LocalMethod(method);
        }
    }
} // end of namespace