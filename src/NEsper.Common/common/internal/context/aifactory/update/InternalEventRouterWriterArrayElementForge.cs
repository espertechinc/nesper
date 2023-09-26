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
using com.espertech.esper.common.@internal.epl.variable.core;
using com.espertech.esper.common.@internal.util;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.context.aifactory.update
{
    public class InternalEventRouterWriterArrayElementForge : InternalEventRouterWriterForge
    {
        private readonly ExprNode _indexExpression;
        private readonly ExprNode _rhsExpression;
        private readonly TypeWidenerSPI _widener;
        private readonly string _propertyName;

        public InternalEventRouterWriterArrayElementForge(
            ExprNode indexExpression,
            ExprNode rhsExpression,
            TypeWidenerSPI widener,
            string propertyName)
        {
            _indexExpression = indexExpression;
            _rhsExpression = rhsExpression;
            _widener = widener;
            _propertyName = propertyName;
        }

        public override CodegenExpression Codegen(
            InternalEventRouterWriterForge writer,
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent
                .MakeChild(typeof(InternalEventRouterWriterArrayElement), GetType(), classScope);

            var indexExpr = ExprNodeUtilityCodegen.CodegenEvaluator(
                _indexExpression.Forge,
                method,
                typeof(VariableTriggerWriteArrayElementForge),
                classScope);
            var rhsExpr = ExprNodeUtilityCodegen.CodegenEvaluator(
                _rhsExpression.Forge,
                method,
                typeof(VariableTriggerWriteArrayElementForge),
                classScope);
            var typeWidenerExpr = _widener == null
                ? ConstantNull()
                : TypeWidenerFactory.CodegenWidener(_widener, method, GetType(), classScope);

            method.Block
                .DeclareVar<InternalEventRouterWriterArrayElement>(
                    "desc",
                    NewInstance(typeof(InternalEventRouterWriterArrayElement)))
                .SetProperty(Ref("desc"), "IndexExpression", indexExpr)
                .SetProperty(Ref("desc"), "RhsExpression", rhsExpr)
                .SetProperty(Ref("desc"), "TypeWidener", typeWidenerExpr)
                .SetProperty(Ref("desc"), "PropertyName", Constant(_propertyName))
                .MethodReturn(Ref("desc"));
            return LocalMethod(method);
        }
    }
} // end of namespace