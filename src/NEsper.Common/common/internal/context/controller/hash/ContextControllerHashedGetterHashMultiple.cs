///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat.logging;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.context.controller.hash
{
    public class ContextControllerHashedGetterHashMultiple : EventPropertyValueGetterForge
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly int granularity;
        private readonly ExprNode[] nodes;

        public ContextControllerHashedGetterHashMultiple(
            IList<ExprNode> expressions,
            int granularity)
        {
            nodes = ExprNodeUtilityQuery.ToArray(expressions);
            this.granularity = granularity;
        }

        public CodegenExpression EventBeanGetCodegen(
            CodegenExpression beanExpression,
            CodegenMethodScope parent,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(object), GetType(), classScope)
                .AddParam<EventBean>("eventBean");
            method.Block.DeclareVar<EventBean[]>(
                "events",
                NewArrayWithInit(typeof(EventBean), Ref("eventBean")));

            // method to evaluate expressions and compute hash
            var exprSymbol = new ExprForgeCodegenSymbol(true, true);
            var exprMethod = method
                .MakeChildWithScope(typeof(object), typeof(CodegenLegoMethodExpression), exprSymbol, classScope)
                .AddParam(ExprForgeCodegenNames.PARAMS);

            var expressions = new CodegenExpression[nodes.Length];
            for (var i = 0; i < nodes.Length; i++) {
                expressions[i] = nodes[i].Forge.EvaluateCodegen(typeof(object), exprMethod, exprSymbol, classScope);
            }

            exprSymbol.DerivedSymbolsCodegen(method, exprMethod.Block, classScope);

            var hashCode = Ref("hashCode");
            exprMethod.Block.DeclareVar<int>(hashCode.Ref, Constant(0));
            for (var i = 0; i < nodes.Length; i++) {
                var result = Ref("result" + i);
                exprMethod.Block
                    .DeclareVar<object>(result.Ref, expressions[i])
                    .IfRefNotNull(result.Ref)
                    .AssignRef(hashCode, Op(Op(Constant(31), "*", hashCode), "+", ExprDotMethod(result, "GetHashCode")))
                    .BlockEnd();
            }

            exprMethod.Block
                .IfCondition(Relational(hashCode, CodegenExpressionRelational.CodegenRelational.GE, Constant(0)))
                .BlockReturn(Op(hashCode, "%", Constant(granularity)))
                .MethodReturn(Op(Op(hashCode, "%", Constant(granularity)), "*", Constant(-1)));

            method.Block.MethodReturn(LocalMethod(exprMethod, Ref("events"), ConstantTrue(), ConstantNull()));
            return LocalMethod(method, beanExpression);
        }
    }
} // end of namespace