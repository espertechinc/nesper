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
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.enummethod.codegen;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.arr;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval
{
    public class EnumTakeWhileScalarForgeEval : EnumEval
    {
        private readonly EnumTakeWhileScalarForge forge;
        private readonly ExprEvaluator innerExpression;

        public EnumTakeWhileScalarForgeEval(
            EnumTakeWhileScalarForge forge,
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
            if (enumcoll.IsEmpty()) {
                return enumcoll;
            }

            ObjectArrayEventBean evalEvent = new ObjectArrayEventBean(new object[1], forge.type);
            eventsLambda[forge.streamNumLambda] = evalEvent;
            object[] props = evalEvent.Properties;

            if (enumcoll.Count == 1) {
                object item = enumcoll.First();
                props[0] = item;

                object pass = innerExpression.Evaluate(eventsLambda, isNewData, context);
                if (pass == null || false.Equals(pass)) {
                    return Collections.GetEmptyList<object>();
                }

                return Collections.SingletonList(item);
            }

            ArrayDeque<object> result = new ArrayDeque<object>();

            foreach (object next in enumcoll) {
                props[0] = next;

                object pass = innerExpression.Evaluate(eventsLambda, isNewData, context);
                if (pass == null || false.Equals(pass)) {
                    break;
                }

                result.Add(next);
            }

            return result;
        }

        public static CodegenExpression Codegen(
            EnumTakeWhileScalarForge forge,
            EnumForgeCodegenParams args,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            CodegenExpressionField typeMember = codegenClassScope.AddFieldUnshared(
                true, typeof(ObjectArrayEventType),
                Cast(typeof(ObjectArrayEventType), EventTypeUtility.ResolveTypeCodegen(forge.type, EPStatementInitServicesConstants.REF)));

            ExprForgeCodegenSymbol scope = new ExprForgeCodegenSymbol(false, null);
            CodegenMethod methodNode = codegenMethodScope
                .MakeChildWithScope(typeof(ICollection<object>), typeof(EnumTakeWhileScalarForgeEval), scope, codegenClassScope)
                .AddParam(EnumForgeCodegenNames.PARAMS);

            CodegenExpression innerValue = forge.innerExpression.EvaluateCodegen(typeof(bool?), methodNode, scope, codegenClassScope);
            CodegenBlock block = methodNode.Block
                .IfCondition(ExprDotMethod(EnumForgeCodegenNames.REF_ENUMCOLL, "isEmpty"))
                .BlockReturn(EnumForgeCodegenNames.REF_ENUMCOLL);
            block.DeclareVar(
                    typeof(ObjectArrayEventBean), "evalEvent",
                    NewInstance(typeof(ObjectArrayEventBean), NewArrayByLength(typeof(object), Constant(1)), typeMember))
                .AssignArrayElement(EnumForgeCodegenNames.REF_EPS, Constant(forge.streamNumLambda), @Ref("evalEvent"))
                .DeclareVar(typeof(object[]), "props", ExprDotMethod(@Ref("evalEvent"), "getProperties"));

            CodegenBlock blockSingle = block.IfCondition(EqualsIdentity(ExprDotMethod(EnumForgeCodegenNames.REF_ENUMCOLL, "size"), Constant(1)))
                .DeclareVar(typeof(object), "item", ExprDotMethodChain(EnumForgeCodegenNames.REF_ENUMCOLL).Add("iterator").Add("next"))
                .AssignArrayElement("props", Constant(0), @Ref("item"));
            CodegenLegoBooleanExpression.CodegenReturnValueIfNotNullAndNotPass(
                blockSingle, forge.innerExpression.EvaluationType, innerValue, StaticMethod(typeof(Collections), "emptyList"));
            blockSingle.BlockReturn(StaticMethod(typeof(Collections), "singletonList", @Ref("item")));

            block.DeclareVar(typeof(ArrayDeque<object>), "result", NewInstance(typeof(ArrayDeque<object>)));
            CodegenBlock forEach = block.ForEach(typeof(object), "next", EnumForgeCodegenNames.REF_ENUMCOLL)
                .AssignArrayElement("props", Constant(0), @Ref("next"));
            CodegenLegoBooleanExpression.CodegenBreakIfNotNullAndNotPass(forEach, forge.innerExpression.EvaluationType, innerValue);
            forEach.Expression(ExprDotMethod(@Ref("result"), "add", @Ref("next")));
            block.MethodReturn(@Ref("result"));
            return LocalMethod(methodNode, args.Eps, args.Enumcoll, args.IsNewData, args.ExprCtx);
        }
    }
} // end of namespace