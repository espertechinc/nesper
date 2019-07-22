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

namespace com.espertech.esper.common.@internal.epl.enummethod.eval
{
    public class EnumOrderByAscDescScalarLambdaForgeEval : EnumEval
    {
        private readonly EnumOrderByAscDescScalarLambdaForge forge;
        private readonly ExprEvaluator innerExpression;

        public EnumOrderByAscDescScalarLambdaForgeEval(
            EnumOrderByAscDescScalarLambdaForge forge,
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
            OrderedDictionary<IComparable, object> sort = new OrderedDictionary<IComparable, object>();
            bool hasColl = false;

            ObjectArrayEventBean resultEvent = new ObjectArrayEventBean(new object[1], forge.resultEventType);
            eventsLambda[forge.streamNumLambda] = resultEvent;
            object[] props = resultEvent.Properties;

            ICollection<object> values = (ICollection<object>) enumcoll;
            foreach (object next in values) {
                props[0] = next;

                IComparable comparable = (IComparable) innerExpression.Evaluate(eventsLambda, isNewData, context);
                object entry = sort.Get(comparable);

                if (entry == null) {
                    sort.Put(comparable, next);
                    continue;
                }

                if (entry is ICollection<object>) {
                    ((ICollection<object>) entry).Add(next);
                    continue;
                }

                Deque<object> coll = new ArrayDeque<object>();
                coll.Add(entry);
                coll.Add(next);
                sort.Put(comparable, coll);
                hasColl = true;
            }

            return EnumOrderByAscDescEventsForgeEval.EnumOrderBySortEval(sort, hasColl, forge.descending);
        }

        public static CodegenExpression Codegen(
            EnumOrderByAscDescScalarLambdaForge forge,
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
            Type innerBoxedType = Boxing.GetBoxedType(forge.innerExpression.EvaluationType);

            ExprForgeCodegenSymbol scope = new ExprForgeCodegenSymbol(false, null);
            CodegenMethod methodNode = codegenMethodScope.MakeChildWithScope(
                    typeof(ICollection<object>),
                    typeof(EnumOrderByAscDescScalarLambdaForgeEval),
                    scope,
                    codegenClassScope)
                .AddParam(EnumForgeCodegenNames.PARAMS);

            CodegenBlock block = methodNode.Block
                .DeclareVar<OrderedDictionary<object, object>>(
                    "sort",
                    NewInstance(typeof(OrderedDictionary<object, object>)))
                .DeclareVar<bool>("hasColl", ConstantFalse())
                .DeclareVar<ObjectArrayEventBean>(
                    "resultEvent",
                    NewInstance<ObjectArrayEventBean>(NewArrayByLength(typeof(object), Constant(1)), resultTypeMember))
                .AssignArrayElement(EnumForgeCodegenNames.REF_EPS, Constant(forge.streamNumLambda), @Ref("resultEvent"))
                .DeclareVar<object[]>("props", ExprDotMethod(@Ref("resultEvent"), "getProperties"));

            block.ForEach(typeof(object), "next", EnumForgeCodegenNames.REF_ENUMCOLL)
                .AssignArrayElement("props", Constant(0), @Ref("next"))
                .DeclareVar(
                    innerBoxedType,
                    "value",
                    forge.innerExpression.EvaluateCodegen(innerBoxedType, methodNode, scope, codegenClassScope))
                .DeclareVar<object>("entry", ExprDotMethod(@Ref("sort"), "get", @Ref("value")))
                .IfCondition(EqualsNull(@Ref("entry")))
                .Expression(ExprDotMethod(@Ref("sort"), "put", @Ref("value"), @Ref("next")))
                .BlockContinue()
                .IfCondition(InstanceOf(@Ref("entry"), typeof(ICollection<object>)))
                .ExprDotMethod(Cast(typeof(ICollection<object>), @Ref("entry")), "add", @Ref("next"))
                .BlockContinue()
                .DeclareVar<Deque<object>>("coll", NewInstance<ArrayDeque<object>>(Constant(2)))
                .ExprDotMethod(@Ref("coll"), "add", @Ref("entry"))
                .ExprDotMethod(@Ref("coll"), "add", @Ref("next"))
                .ExprDotMethod(@Ref("sort"), "put", @Ref("value"), @Ref("coll"))
                .AssignRef("hasColl", ConstantTrue())
                .BlockEnd();
            block.MethodReturn(
                StaticMethod(
                    typeof(EnumOrderByAscDescEventsForgeEval),
                    "enumOrderBySortEval",
                    @Ref("sort"),
                    @Ref("hasColl"),
                    Constant(forge.descending)));
            return LocalMethod(methodNode, args.Eps, args.Enumcoll, args.IsNewData, args.ExprCtx);
        }
    }
} // end of namespace