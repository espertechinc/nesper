///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.enummethod.codegen;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionRelational.
    CodegenRelational;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval
{
    public class EnumDistinctEventsForgeEval : EnumEval
    {
        private readonly EnumDistinctEventsForge _forge;
        private readonly ExprEvaluator _innerExpression;

        public EnumDistinctEventsForgeEval(
            EnumDistinctEventsForge forge,
            ExprEvaluator innerExpression)
        {
            _forge = forge;
            _innerExpression = innerExpression;
        }

        public object EvaluateEnumMethod(
            EventBean[] eventsLambda,
            ICollection<object> enumcoll,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            var beans = (ICollection<EventBean>) enumcoll;
            if (beans.Count <= 1) {
                return beans;
            }

            IDictionary<IComparable, EventBean> distinct = new LinkedHashMap<IComparable, EventBean>();
            foreach (var next in beans) {
                eventsLambda[_forge.StreamNumLambda] = next;

                var comparable = (IComparable) _innerExpression.Evaluate(eventsLambda, isNewData, context);
                if (!distinct.ContainsKey(comparable)) {
                    distinct.Put(comparable, next);
                }
            }

            return distinct.Values;
        }

        public static CodegenExpression Codegen(
            EnumDistinctEventsForge forge,
            EnumForgeCodegenParams args,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            var innerType = Boxing.GetBoxedType(forge.InnerExpression.EvaluationType);

            var scope = new ExprForgeCodegenSymbol(false, null);
            var methodNode = codegenMethodScope.MakeChildWithScope(
                    typeof(ICollection<EventBean>),
                    typeof(EnumDistinctEventsForgeEval),
                    scope,
                    codegenClassScope)
                .AddParam(EnumForgeCodegenNames.PARAMS_EVENTBEAN);

            var block = methodNode.Block
                .IfCondition(Relational(ExprDotName(EnumForgeCodegenNames.REF_ENUMCOLL, "Count"), LE, Constant(1)))
                .BlockReturn(EnumForgeCodegenNames.REF_ENUMCOLL)
                .DeclareVar<IDictionary<object, EventBean>>("distinct", NewInstance(typeof(LinkedHashMap<object, EventBean>)));
            block.ForEach(typeof(EventBean), "next", EnumForgeCodegenNames.REF_ENUMCOLL)
                .AssignArrayElement(EnumForgeCodegenNames.REF_EPS, Constant(forge.StreamNumLambda), @Ref("next"))
                .DeclareVar(
                    innerType,
                    "comparable",
                    forge.InnerExpression.EvaluateCodegen(innerType, methodNode, scope, codegenClassScope))
                .IfCondition(Not(ExprDotMethod(@Ref("distinct"), "ContainsKey", @Ref("comparable"))))
                .Expression(ExprDotMethod(@Ref("distinct"), "Put", @Ref("comparable"), @Ref("next")))
                .BlockEnd();
            block.MethodReturn(ExprDotName(@Ref("distinct"), "Values"));
            return LocalMethod(methodNode, args.Eps, args.Enumcoll, args.IsNewData, args.ExprCtx);
        }
    }
} // end of namespace