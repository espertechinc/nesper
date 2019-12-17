///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.metrics.instrumentation
{
    public class InstrumentationBuilderExpr
    {
        private readonly Type generator;
        private readonly ExprForgeInstrumentable forge;
        private readonly string qname;
        private readonly Type requiredType;
        private readonly CodegenMethodScope codegenMethodScope;
        private readonly ExprForgeCodegenSymbol exprSymbol;
        private readonly CodegenClassScope codegenClassScope;
        private readonly IList<CodegenExpression> qParams = new List<CodegenExpression>();

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

            string text = ExprNodeUtilityPrint.ToExpressionStringMinPrecedence(forge);
            this.qParams.Insert(0, Constant(text));
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
            if (evaluationType == typeof(void)) {
                return ConstantNull();
            }

            if (evaluationType == null) {
                CodegenMethod methodX = codegenMethodScope.MakeChild(typeof(object), generator, codegenClassScope);
                methodX.Block
                    .IfCondition(PublicConstValue(InstrumentationConstants.RUNTIME_HELPER_CLASS, "ENABLED"))
                    .Expression(
                        ExprDotMethodChain(StaticMethod(InstrumentationConstants.RUNTIME_HELPER_CLASS, "Get"))
                            .Add("q" + qname, qParams.ToArray()))
                    .Expression(
                        ExprDotMethodChain(StaticMethod(InstrumentationConstants.RUNTIME_HELPER_CLASS, "Get"))
                            .Add("a" + qname, ConstantNull()))
                    .BlockEnd()
                    .MethodReturn(ConstantNull());
                return LocalMethod(methodX);
            }

            CodegenMethod method = codegenMethodScope.MakeChild(evaluationType, generator, codegenClassScope);
            CodegenExpression expr = forge.EvaluateCodegenUninstrumented(
                evaluationType,
                method,
                exprSymbol,
                codegenClassScope);
            method.Block
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
            return LocalMethod(method);
        }

        public InstrumentationBuilderExpr Noqparam()
        {
            qParams.Clear();
            return this;
        }

        public InstrumentationBuilderExpr Qparam(CodegenExpression qparam)
        {
            this.qParams.Add(qparam);
            return this;
        }

        public InstrumentationBuilderExpr Qparams(params CodegenExpression[] qparams)
        {
            this.qParams.AddAll(qparams);
            return this;
        }
    }
} // end of namespace