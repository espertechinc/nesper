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
using com.espertech.esper.common.@internal.rettype;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval
{
    public class EnumLastOfPredicateScalarForgeEval : EnumEval
    {
        private readonly EnumLastOfPredicateScalarForge forge;
        private readonly ExprEvaluator innerExpression;

        public EnumLastOfPredicateScalarForgeEval(
            EnumLastOfPredicateScalarForge forge,
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
            object result = null;
            ObjectArrayEventBean evalEvent = new ObjectArrayEventBean(new object[1], forge.type);
            eventsLambda[forge.streamNumLambda] = evalEvent;
            object[] props = evalEvent.Properties;

            foreach (object next in enumcoll) {
                props[0] = next;

                object pass = innerExpression.Evaluate(eventsLambda, isNewData, context);
                if (pass == null || false.Equals(pass)) {
                    continue;
                }

                result = next;
            }

            return result;
        }

        public static CodegenExpression Codegen(
            EnumLastOfPredicateScalarForge forge,
            EnumForgeCodegenParams args,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            CodegenExpressionField typeMember = codegenClassScope.AddFieldUnshared(
                true, typeof(ObjectArrayEventType),
                Cast(typeof(ObjectArrayEventType), EventTypeUtility.ResolveTypeCodegen(forge.type, EPStatementInitServicesConstants.REF)));
            Type resultType = Boxing.GetBoxedType(EPTypeHelper.GetCodegenReturnType(forge.resultType));

            ExprForgeCodegenSymbol scope = new ExprForgeCodegenSymbol(false, null);
            CodegenMethod methodNode = codegenMethodScope
                .MakeChildWithScope(resultType, typeof(EnumLastOfPredicateScalarForgeEval), scope, codegenClassScope)
                .AddParam(EnumForgeCodegenNames.PARAMS);

            CodegenBlock block;
            block = methodNode.Block
                .DeclareVar(typeof(object), "result", ConstantNull())
                .DeclareVar(
                    typeof(ObjectArrayEventBean), "evalEvent",
                    NewInstance(typeof(ObjectArrayEventBean), NewArrayByLength(typeof(object), Constant(1)), typeMember))
                .AssignArrayElement(EnumForgeCodegenNames.REF_EPS, Constant(forge.streamNumLambda), @Ref("evalEvent"))
                .DeclareVar(typeof(object[]), "props", ExprDotMethod(@Ref("evalEvent"), "getProperties"));
            CodegenBlock forEach = block.ForEach(typeof(object), "next", EnumForgeCodegenNames.REF_ENUMCOLL)
                .AssignArrayElement("props", Constant(0), @Ref("next"));
            CodegenLegoBooleanExpression.CodegenContinueIfNotNullAndNotPass(
                forEach, forge.innerExpression.EvaluationType,
                forge.innerExpression.EvaluateCodegen(typeof(bool?), methodNode, scope, codegenClassScope));
            forEach.AssignRef("result", @Ref("next"));
            block.MethodReturn(Cast(resultType, @Ref("result")));
            return LocalMethod(methodNode, args.Eps, args.Enumcoll, args.IsNewData, args.ExprCtx);
        }
    }
} // end of namespace