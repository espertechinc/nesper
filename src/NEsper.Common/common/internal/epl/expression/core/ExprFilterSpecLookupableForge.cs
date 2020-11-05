///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.serde.compiletime.resolve;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.core
{
    public class ExprFilterSpecLookupableForge
    {
        private readonly string _expression;
        private readonly ExprEventEvaluatorForge _optionalEventEvalForge;
        private readonly ExprForge _optionalExprForge;
        private readonly Type _returnType;
        private readonly bool _isNonPropertyGetter;
        private readonly DataInputOutputSerdeForge _valueSerde;

        public ExprFilterSpecLookupableForge(
            string expression,
            ExprEventEvaluatorForge optionalEventEvalForge,
            ExprForge optionalExprForge,
            Type returnType,
            bool isNonPropertyGetter,
            DataInputOutputSerdeForge valueSerde)
        {
            // prefixing the expression ensures the expression resolves to either the event-eval or the expr-eval
            _expression = optionalExprForge != null ? "." + expression : expression;
            _optionalEventEvalForge = optionalEventEvalForge;
            _optionalExprForge = optionalExprForge;
            _returnType = Boxing.GetBoxedType(returnType); // For type consistency for recovery and serde define as boxed type
            _isNonPropertyGetter = isNonPropertyGetter;
            _valueSerde = valueSerde;
        }
        public Type ReturnType => _returnType;

        public string Expression => _expression;

        public virtual CodegenMethod MakeCodegen(
            CodegenMethodScope parent,
            SAIFFInitializeSymbolWEventType symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(
                typeof(ExprFilterSpecLookupable),
                typeof(ExprFilterSpecLookupableForge),
                classScope);
            CodegenExpression singleEventEvalExpr = ConstantNull();
            if (_optionalEventEvalForge != null) {
                var eval = new CodegenExpressionLambda(method.Block)
                    .WithParam<EventBean>("bean")
                    .WithParam<ExprEvaluatorContext>("ctx");
                var anonymous = NewInstance<ProxyExprEventEvaluator>(eval);
                eval.Block.BlockReturn(
                    _optionalEventEvalForge.EventBeanWithCtxGet(Ref("bean"), Ref("ctx"), method, classScope));
                singleEventEvalExpr = anonymous;
            }

            CodegenExpression epsEvalExpr = ConstantNull();
            if (_optionalExprForge != null) {
                epsEvalExpr = ExprNodeUtilityCodegen.CodegenEvaluator(
                    _optionalExprForge, method, typeof(ExprFilterSpecLookupableForge), classScope);
            }

            CodegenExpression serdeExpr = _valueSerde == null ? ConstantNull() : _valueSerde.Codegen(method, classScope, null);
            CodegenExpression returnTypeExpr = _returnType == null ? ConstantNull() : Typeof(_returnType);

            method.Block
                .DeclareVar<ExprEventEvaluator>("eval", singleEventEvalExpr)
                .DeclareVar<ExprEvaluator>("expr", epsEvalExpr)
                .DeclareVar<ExprFilterSpecLookupable>(
                    "lookupable",
                    NewInstance<ExprFilterSpecLookupable>(
                        Constant(_expression),
                        Ref("eval"),
                        Ref("expr"),
                        returnTypeExpr,
                        Constant(_isNonPropertyGetter),
                        serdeExpr))
                .Expression(
                    ExprDotMethodChain(symbols.GetAddInitSvc(method))
                        .Get(EPStatementInitServicesConstants.FILTERSHAREDLOOKUPABLEREGISTERY)
                        .Add(
                            "RegisterLookupable",
                            symbols.GetAddEventType(method),
                            Ref("lookupable")))
                .MethodReturn(Ref("lookupable"));
            return method;
        }

        public override bool Equals(object o)
        {
            if (this == o) {
                return true;
            }

            if (o == null || GetType() != o.GetType()) {
                return false;
            }

            var that = (ExprFilterSpecLookupableForge) o;

            if (!_expression.Equals(that._expression)) {
                return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            return _expression.GetHashCode();
        }
    }
} // end of namespace