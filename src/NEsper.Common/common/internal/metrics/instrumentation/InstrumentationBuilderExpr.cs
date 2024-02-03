///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.metrics.instrumentation
{
    public class InstrumentationBuilderExpr
    {
        private readonly CodegenClassScope codegenClassScope;
        private readonly CodegenMethodScope codegenMethodScope;
        private readonly ExprForgeCodegenSymbol exprSymbol;
        private readonly ExprForgeInstrumentable forge;
        private readonly Type generator;
        private readonly string qname;
        private readonly IList<CodegenExpression> qParams = new List<CodegenExpression>();
        private readonly Type requiredType;

        public InstrumentationBuilderExpr(
            Type generator,
            ExprForgeInstrumentable forge,
            string qname,
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            this.generator = generator;
            this.forge = forge;
            this.qname = qname;
            this.requiredType = requiredType;
            this.codegenMethodScope = codegenMethodScope;
            this.exprSymbol = exprSymbol;
            this.codegenClassScope = codegenClassScope;

            var text = ExprNodeUtilityPrint.ToExpressionStringMinPrecedence(forge);
            qParams.Insert(0, Constant(text));
        }

        public CodegenExpression Build()
        {
            if (!codegenClassScope.IsInstrumented) {
                return forge.EvaluateCodegenUninstrumented(
                    requiredType,
                    codegenMethodScope,
                    exprSymbol,
                    codegenClassScope);
            }

            var evaluationType = forge.EvaluationType;
            if (evaluationType != null && evaluationType.IsTypeVoid()) {
                return ConstantNull();
            }

            if (evaluationType == null) {
                var method = codegenMethodScope.MakeChild(typeof(object), generator, codegenClassScope);
                method.Block
                    .IfCondition(PublicConstValue(InstrumentationConstants.RUNTIME_HELPER_CLASS, "ENABLED"))
                    .Expression(
                        ExprDotMethodChain(StaticMethod(InstrumentationConstants.RUNTIME_HELPER_CLASS, "Get"))
                            .Add("q" + qname, qParams.ToArray()))
                    .Expression(
                        ExprDotMethodChain(StaticMethod(InstrumentationConstants.RUNTIME_HELPER_CLASS, "Get"))
                            .Add("a" + qname, ConstantNull()))
                    .BlockEnd()
                    .MethodReturn(ConstantNull());
                return LocalMethod(method);
            }

            var evalTypeClass = evaluationType;
            var methodX = codegenMethodScope.MakeChild(evalTypeClass, generator, codegenClassScope);
            var expr = forge.EvaluateCodegenUninstrumented(
                evalTypeClass,
                methodX,
                exprSymbol,
                codegenClassScope);
            methodX.Block
                .IfCondition(PublicConstValue(InstrumentationConstants.RUNTIME_HELPER_CLASS, "ENABLED"))
                .Expression(
                    ExprDotMethodChain(StaticMethod(InstrumentationConstants.RUNTIME_HELPER_CLASS, "Get"))
                        .Add("q" + qname, qParams.ToArray()))
                .DeclareVar(evaluationType, "result", expr)
                .Expression(
                    ExprDotMethodChain(StaticMethod(InstrumentationConstants.RUNTIME_HELPER_CLASS, "Get"))
                        .Add("a" + qname, Ref("result")))
                .BlockReturn(Ref("result"))
                .MethodReturn(expr);
            return LocalMethod(methodX);
        }

        public InstrumentationBuilderExpr Noqparam()
        {
            qParams.Clear();
            return this;
        }

        public InstrumentationBuilderExpr Qparam(CodegenExpression qparam)
        {
            qParams.Add(qparam);
            return this;
        }

        public InstrumentationBuilderExpr Qparams(params CodegenExpression[] qparams)
        {
            qParams.AddAll(Arrays.AsList(qparams));
            return this;
        }
    }
} // end of namespace