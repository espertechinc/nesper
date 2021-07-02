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

using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.metrics.instrumentation
{
    public class InstrumentationBuilderExpr
    {
        private readonly Type _generator;
        private readonly ExprForgeInstrumentable _forge;
        private readonly string _qname;
        private readonly Type _requiredType;
        private readonly CodegenMethodScope _codegenMethodScope;
        private readonly ExprForgeCodegenSymbol _exprSymbol;
        private readonly CodegenClassScope _codegenClassScope;
        private readonly IList<CodegenExpression> _qParams = new List<CodegenExpression>();

        public InstrumentationBuilderExpr(
            Type generator,
            ExprForgeInstrumentable forge,
            string qname,
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            this._generator = generator;
            this._forge = forge;
            this._qname = qname;
            this._requiredType = requiredType;
            this._codegenMethodScope = codegenMethodScope;
            this._exprSymbol = exprSymbol;
            this._codegenClassScope = codegenClassScope;

            var text = ExprNodeUtilityPrint.ToExpressionStringMinPrecedence(forge);
            _qParams.Insert(0, Constant(text));
        }

        public CodegenExpression Build()
        {
            if (!_codegenClassScope.IsInstrumented) {
                return _forge.EvaluateCodegenUninstrumented(
                    _requiredType,
                    _codegenMethodScope,
                    _exprSymbol,
                    _codegenClassScope);
            }

            var evaluationType = _forge.EvaluationType;
            if (evaluationType != null && evaluationType.IsVoid()) {
                return ConstantNull();
            }

            if (evaluationType.IsNullTypeSafe()) {
                var methodX = _codegenMethodScope.MakeChild(typeof(object), _generator, _codegenClassScope);
                methodX.Block
                    .IfCondition(PublicConstValue(InstrumentationConstants.RUNTIME_HELPER_CLASS, "ENABLED"))
                    .Expression(
                        ExprDotMethodChain(StaticMethod(InstrumentationConstants.RUNTIME_HELPER_CLASS, "Get"))
                            .Add("q" + _qname, _qParams.ToArray()))
                    .Expression(
                        ExprDotMethodChain(StaticMethod(InstrumentationConstants.RUNTIME_HELPER_CLASS, "Get"))
                            .Add("a" + _qname, ConstantNull()))
                    .BlockEnd()
                    .MethodReturn(ConstantNull());
                return LocalMethod(methodX);
            }

            var method = _codegenMethodScope.MakeChild(evaluationType, _generator, _codegenClassScope);
            var expr = _forge.EvaluateCodegenUninstrumented(
                evaluationType,
                method,
                _exprSymbol,
                _codegenClassScope);
            method.Block
                .IfCondition(PublicConstValue(InstrumentationConstants.RUNTIME_HELPER_CLASS, "ENABLED"))
                .Expression(
                    ExprDotMethodChain(StaticMethod(InstrumentationConstants.RUNTIME_HELPER_CLASS, "Get"))
                        .Add("q" + _qname, _qParams.ToArray()))
                .DeclareVar(evaluationType, "result", expr)
                .Expression(
                    ExprDotMethodChain(StaticMethod(InstrumentationConstants.RUNTIME_HELPER_CLASS, "Get"))
                        .Add("a" + _qname, Ref("result")))
                .BlockReturn(Ref("result"))
                .MethodReturn(expr);
            return LocalMethod(method);
        }

        public InstrumentationBuilderExpr Noqparam()
        {
            _qParams.Clear();
            return this;
        }

        public InstrumentationBuilderExpr Qparam(CodegenExpression qparam)
        {
            _qParams.Add(qparam);
            return this;
        }

        public InstrumentationBuilderExpr Qparams(params CodegenExpression[] qparams)
        {
            _qParams.AddAll(qparams);
            return this;
        }
    }
} // end of namespace