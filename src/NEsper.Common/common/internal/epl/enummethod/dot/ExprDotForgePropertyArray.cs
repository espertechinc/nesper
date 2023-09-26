///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.dot.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.rettype;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.expression.codegen.CodegenLegoCast; // CastSafeFromObjectType

namespace com.espertech.esper.common.@internal.epl.enummethod.dot
{
    public class ExprDotForgePropertyArray : ExprDotEval,
        ExprDotForge
    {
        private readonly EventPropertyGetterSPI _getter;
        private readonly EPChainableType _returnType;
        private readonly ExprNode _indexExpression;
        private readonly Type _arrayType;
        private readonly string _propertyName;

        public ExprDotForgePropertyArray(
            EventPropertyGetterSPI getter,
            EPChainableType returnType,
            ExprNode indexExpression,
            Type arrayType,
            string propertyName)
        {
            _getter = getter;
            _returnType = returnType;
            _indexExpression = indexExpression;
            _arrayType = arrayType;
            _propertyName = propertyName;
        }

        public object Evaluate(
            object target,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            if (!(target is EventBean eventBean)) {
                return null;
            }

            var array = _getter.Get(eventBean) as Array;
            if (array == null) {
                return null;
            }

            var index = _indexExpression.Forge.ExprEvaluator.Evaluate(eventsPerStream, isNewData, exprEvaluatorContext)
                .AsBoxedInt32();
            if (index == null) {
                return null;
            }

            return array.GetValue(index.Value);
        }

        public EPChainableType TypeInfo => _returnType;

        public void Visit(ExprDotEvalVisitor visitor)
        {
            visitor.VisitPropertySource();
        }

        public ExprDotEval DotEvaluator => this;

        public ExprDotForge DotForge => this;

        public CodegenExpression Codegen(
            CodegenExpression inner,
            Type innerType,
            CodegenMethodScope parent,
            ExprForgeCodegenSymbol symbols,
            CodegenClassScope classScope)
        {
            var type = _returnType.GetCodegenReturnType();
            var method = parent
                .MakeChild(type, typeof(ExprDotForgeProperty), classScope)
                .AddParam(innerType, "target")
                .AddParam(typeof(int?), "index");

            var arrayExpr = CastSafeFromObjectType(
                _arrayType,
                _getter.EventBeanGetCodegen(Cast(typeof(EventBean), Ref("target")), method, classScope));

            method.Block
                .IfNotInstanceOf("target", typeof(EventBean))
                .BlockReturn(ConstantNull())
                .DeclareVar(_arrayType, "array", arrayExpr)
                .IfRefNullReturnNull("index")
                .IfCondition(
                    Relational(
                        Ref("index"),
                        CodegenExpressionRelational.CodegenRelational.GE,
                        ArrayLength(Ref("array"))))
                .BlockThrow(
                    NewInstance<EPException>(
                        Concat(
                            Constant("Array length "),
                            ArrayLength(Ref("array")),
                            Constant(" less than index "),
                            Ref("index"),
                            Constant(" for property '" + _propertyName + "'"))))
                .MethodReturn(
                    CastSafeFromObjectType(type, ArrayAtIndex(Ref("array"), ExprDotMethod(Ref("index"), "AsInt32"))));

            return LocalMethod(
                method,
                inner,
                _indexExpression.Forge.EvaluateCodegen(typeof(int?), method, symbols, classScope));
        }
    }
} // end of namespace