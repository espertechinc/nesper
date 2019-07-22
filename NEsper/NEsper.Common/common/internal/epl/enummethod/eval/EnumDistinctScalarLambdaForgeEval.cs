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
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.enummethod.codegen;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.arr;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionRelational.
    CodegenRelational;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval
{
    public class EnumDistinctScalarLambdaForgeEval : EnumEval
    {
        private readonly EnumDistinctScalarLambdaForge forge;
        private readonly ExprEvaluator innerExpression;

        public EnumDistinctScalarLambdaForgeEval(
            EnumDistinctScalarLambdaForge forge,
            ExprEvaluator innerExpression)
        {
            this.forge = forge;
            this.innerExpression = innerExpression;
        }

        public object EvaluateEnumMethod(
            EventBean[] eventsLambda,
            ICollection<object> enumcoll,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            if (enumcoll.Count <= 1) {
                return enumcoll;
            }

            IDictionary<IComparable, object> distinct = new LinkedHashMap<IComparable, object>();
            ObjectArrayEventBean resultEvent = new ObjectArrayEventBean(new object[1], forge.resultEventType);
            eventsLambda[forge.streamNumLambda] = resultEvent;
            object[] props = resultEvent.Properties;

            ICollection<object> values = (ICollection<object>) enumcoll;
            foreach (object next in values) {
                props[0] = next;

                IComparable comparable = (IComparable) innerExpression.Evaluate(eventsLambda, isNewData, context);
                if (!distinct.ContainsKey(comparable)) {
                    distinct.Put(comparable, next);
                }
            }

            return distinct.Values;
        }

        public static CodegenExpression Codegen(
            EnumDistinctScalarLambdaForge forge,
            EnumForgeCodegenParams args,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            CodegenExpressionField resultTypeMember = codegenClassScope.AddFieldUnshared(
                true,
                typeof(ObjectArrayEventType),
                Cast(
                    typeof(ObjectArrayEventType),
                    EventTypeUtility.ResolveTypeCodegen(forge.resultEventType, EPStatementInitServicesConstants.REF)));
            Type innerType = Boxing.GetBoxedType(forge.innerExpression.EvaluationType);

            ExprForgeCodegenSymbol scope = new ExprForgeCodegenSymbol(false, null);
            CodegenMethod methodNode = codegenMethodScope
                .MakeChildWithScope(
                    typeof(ICollection<object>),
                    typeof(EnumDistinctScalarLambdaForgeEval),
                    scope,
                    codegenClassScope)
                .AddParam(EnumForgeCodegenNames.PARAMS);

            CodegenBlock block = methodNode.Block
                .IfCondition(Relational(ExprDotMethod(EnumForgeCodegenNames.REF_ENUMCOLL, "size"), LE, Constant(1)))
                .BlockReturn(EnumForgeCodegenNames.REF_ENUMCOLL)
                .DeclareVar<IDictionary<object, object>>("distinct", NewInstance(typeof(LinkedHashMap<object, object>)))
                .DeclareVar<ObjectArrayEventBean>(
                    "resultEvent",
                    NewInstance<ObjectArrayEventBean>(NewArrayByLength(typeof(object), Constant(1)), resultTypeMember))
                .AssignArrayElement(EnumForgeCodegenNames.REF_EPS, Constant(forge.streamNumLambda), @Ref("resultEvent"))
                .DeclareVar<object[]>("props", ExprDotMethod(@Ref("resultEvent"), "getProperties"));

            block.ForEach(typeof(object), "next", EnumForgeCodegenNames.REF_ENUMCOLL)
                .AssignArrayElement("props", Constant(0), @Ref("next"))
                .DeclareVar(
                    innerType,
                    "comparable",
                    forge.innerExpression.EvaluateCodegen(innerType, methodNode, scope, codegenClassScope))
                .IfCondition(Not(ExprDotMethod(@Ref("distinct"), "containsKey", @Ref("comparable"))))
                .Expression(ExprDotMethod(@Ref("distinct"), "put", @Ref("comparable"), @Ref("next")))
                .BlockEnd();
            block.MethodReturn(ExprDotMethod(@Ref("distinct"), "values"));
            return LocalMethod(methodNode, args.Eps, args.Enumcoll, args.IsNewData, args.ExprCtx);
        }
    }
} // end of namespace