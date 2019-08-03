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
using com.espertech.esper.common.@internal.epl.enummethod.cache;
using com.espertech.esper.common.@internal.epl.enummethod.codegen;
using com.espertech.esper.common.@internal.epl.enummethod.eval;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.dot.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.rettype;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.enummethod.dot
{
    public class ExprDotForgeEnumMethodEval : ExprDotEval
    {
        private readonly bool cache;
        private readonly EnumEval enumEval;
        private readonly int enumEvalNumRequiredEvents;

        private readonly ExprDotForgeEnumMethodBase forge;

        public ExprDotForgeEnumMethodEval(
            ExprDotForgeEnumMethodBase forge,
            EnumEval enumEval,
            bool cache,
            int enumEvalNumRequiredEvents)
        {
            this.forge = forge;
            this.enumEval = enumEval;
            this.cache = cache;
            this.enumEvalNumRequiredEvents = enumEvalNumRequiredEvents;
        }

        public EPType TypeInfo => forge.TypeInfo;

        public object Evaluate(
            object target,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            if (target is EventBean) {
                target = Collections.SingletonList((EventBean) target);
            }

            var coll = target.Unwrap<object>();
            if (coll == null) {
                return null;
            }

            var eventsLambda = AllocateCopyEventLambda(eventsPerStream, enumEvalNumRequiredEvents);
            return enumEval.EvaluateEnumMethod(eventsLambda, coll, isNewData, exprEvaluatorContext);
        }

        public ExprDotForge DotForge => forge;

        public static CodegenExpression Codegen(
            ExprDotForgeEnumMethodBase forge,
            CodegenExpression inner,
            Type innerType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            Type returnType = EPTypeHelper.GetCodegenReturnType(forge.TypeInfo);
            var methodNode = codegenMethodScope
                .MakeChild(returnType, typeof(ExprDotForgeEnumMethodEval), codegenClassScope)
                .AddParam(innerType, "param");

            var refEPS = exprSymbol.GetAddEPS(methodNode);
            var refIsNewData = exprSymbol.GetAddIsNewData(methodNode);
            var refExprEvalCtx = exprSymbol.GetAddExprEvalCtx(methodNode);

            var forgeMember = codegenClassScope.AddFieldUnshared(true, typeof(object), NewInstance(typeof(object)));
            var block = methodNode.Block;
            if (innerType == typeof(EventBean)) {
                block.DeclareVar<ICollection<object>>(
                    "coll",
                    StaticMethod(typeof(Collections), "singletonList", Ref("param")));
            }
            else {
                block.DeclareVar<ICollection<object>>("coll", Ref("param"));
            }

            block.DeclareVar<ExpressionResultCacheForEnumerationMethod>(
                "cache",
                ExprDotMethodChain(refExprEvalCtx)
                    .Add("getExpressionResultCacheService")
                    .Add("getAllocateEnumerationMethod"));
            var premade = new EnumForgeCodegenParams(Ref("eventsLambda"), Ref("coll"), refIsNewData, refExprEvalCtx);
            if (forge.cache) {
                block.DeclareVar<ExpressionResultCacheEntryLongArrayAndObj>(
                        "cacheValue",
                        ExprDotMethod(Ref("cache"), "getEnumerationMethodLastValue", forgeMember))
                    .IfCondition(NotEqualsNull(Ref("cacheValue")))
                    .BlockReturn(Cast(returnType, ExprDotName(Ref("cacheValue"), "Result")))
                    .IfRefNullReturnNull("coll")
                    .DeclareVar<EventBean[]>(
                        "eventsLambda",
                        StaticMethod(
                            typeof(ExprDotForgeEnumMethodEval),
                            "allocateCopyEventLambda",
                            refEPS,
                            Constant(forge.enumEvalNumRequiredEvents)))
                    .DeclareVar(
                        EPTypeHelper.GetCodegenReturnType(forge.TypeInfo),
                        "result",
                        forge.enumForge.Codegen(premade, methodNode, codegenClassScope))
                    .Expression(
                        ExprDotMethod(Ref("cache"), "saveEnumerationMethodLastValue", forgeMember, Ref("result")))
                    .MethodReturn(Ref("result"));
            }
            else {
                var contextNumberMember = codegenClassScope.AddFieldUnshared(
                    true,
                    typeof(AtomicLong),
                    NewInstance(typeof(AtomicLong)));
                block.DeclareVar<long>("contextNumber", ExprDotName(contextNumberMember, "AndIncrement"))
                    .TryCatch()
                    .Expression(ExprDotMethod(Ref("cache"), "pushContext", Ref("contextNumber")))
                    .IfRefNullReturnNull("coll")
                    .DeclareVar<EventBean[]>(
                        "eventsLambda",
                        StaticMethod(
                            typeof(ExprDotForgeEnumMethodEval),
                            "allocateCopyEventLambda",
                            refEPS,
                            Constant(forge.enumEvalNumRequiredEvents)))
                    .TryReturn(forge.enumForge.Codegen(premade, methodNode, codegenClassScope))
                    .TryFinally()
                    .Expression(ExprDotMethod(Ref("cache"), "PopContext"))
                    .BlockEnd()
                    .MethodEnd();
            }

            return LocalMethod(methodNode, inner);
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="eventsPerStream">events</param>
        /// <param name="enumEvalNumRequiredEvents">width</param>
        /// <returns>allocated</returns>
        public static EventBean[] AllocateCopyEventLambda(
            EventBean[] eventsPerStream,
            int enumEvalNumRequiredEvents)
        {
            var eventsLambda = new EventBean[enumEvalNumRequiredEvents];
            EventBeanUtility.SafeArrayCopy(eventsPerStream, eventsLambda);
            return eventsLambda;
        }
    }
} // end of namespace