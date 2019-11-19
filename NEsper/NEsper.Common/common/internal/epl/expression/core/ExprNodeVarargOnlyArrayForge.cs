///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.expression.core.ExprNodeUtilityQuery;

namespace com.espertech.esper.common.@internal.epl.expression.core
{
    internal class ExprNodeVarargOnlyArrayForge : ExprForge,
        ExprNodeRenderable
    {
        private readonly ExprForge[] forges;
        internal readonly SimpleNumberCoercer[] optionalCoercers;
        internal readonly Type varargClass;

        public ExprNodeVarargOnlyArrayForge(
            ExprForge[] forges,
            Type varargClass,
            SimpleNumberCoercer[] optionalCoercers)
        {
            this.forges = forges;
            this.varargClass = varargClass;
            this.optionalCoercers = optionalCoercers;
        }

        public ExprEvaluator ExprEvaluator {
            get {
                if (optionalCoercers == null) {
                    return new ExprNodeVarargOnlyArrayEvalNoCoerce(this, GetEvaluatorsNoCompile(forges));
                }

                return new ExprNodeVarargOnlyArrayForgeWithCoerce(this, GetEvaluatorsNoCompile(forges));
            }
        }

        public CodegenExpression EvaluateCodegen(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var arrayType = TypeHelper.GetArrayType(varargClass);
            var methodNode = codegenMethodScope.MakeChild(
                arrayType,
                typeof(ExprNodeVarargOnlyArrayForge),
                codegenClassScope);

            var block = methodNode.Block
                .DeclareVar(arrayType, "array", NewArrayByLength(varargClass, Constant(forges.Length)));
            for (var i = 0; i < forges.Length; i++) {
                var expression = forges[i].EvaluateCodegen(requiredType, methodNode, exprSymbol, codegenClassScope);
                CodegenExpression assignment;
                if (optionalCoercers == null || optionalCoercers[i] == null) {
                    assignment = expression;
                }
                else {
                    var evalType = forges[i].EvaluationType;
                    if (evalType.CanNotBeNull()) {
                        assignment = optionalCoercers[i].CoerceCodegen(expression, evalType);
                    }
                    else {
                        assignment = optionalCoercers[i]
                            .CoerceCodegenMayNullBoxed(
                                expression,
                                evalType,
                                methodNode,
                                codegenClassScope);
                    }
                }

                block.AssignArrayElement("array", Constant(i), assignment);
            }

            block.MethodReturn(Ref("array"));
            return LocalMethod(methodNode);
        }

        public Type EvaluationType => TypeHelper.GetArrayType(varargClass);

        public ExprNodeRenderable ExprForgeRenderable => this;

        public ExprForgeConstantType ForgeConstantType => ExprForgeConstantType.NONCONST;

        public void ToEPL(
            TextWriter writer,
            ExprPrecedenceEnum parentPrecedence)
        {
            writer.Write(GetType().GetSimpleName());
        }
    }
} // end of namespace