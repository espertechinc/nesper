///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;
using com.espertech.esper.common.client;
using com.espertech.esper.common.client.collection;
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
        private readonly ExprDotForgeEnumMethodBase _forge;
        private readonly EnumEval _enumEval;
        private readonly int _enumEvalNumRequiredEvents;

        public ExprDotForgeEnumMethodEval(
            ExprDotForgeEnumMethodBase forge,
            EnumEval enumEval,
            int enumEvalNumRequiredEvents)
        {
            _forge = forge;
            _enumEval = enumEval;
            _enumEvalNumRequiredEvents = enumEvalNumRequiredEvents;
        }

        public object Evaluate(
            object target,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            if (target is EventBean eventBean) {
                target = Collections.SingletonList(eventBean);
            }

            var coll = target.Unwrap<object>();
            if (coll == null) {
                return null;
            }

            var eventsLambda = eventsPerStream == null
                ? Array.Empty<EventBean>()
                : AllocateCopyEventLambda(eventsPerStream, _enumEvalNumRequiredEvents);
            return _enumEval.EvaluateEnumMethod(eventsLambda, coll, isNewData, exprEvaluatorContext);
        }

        public static CodegenExpression Codegen(
            ExprDotForgeEnumMethodBase forge,
            CodegenExpression inner,
            Type innerType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var returnType = forge.TypeInfo.GetCodegenReturnType().GetBoxedType();
            var methodNode = codegenMethodScope
                .MakeChild(returnType, typeof(ExprDotForgeEnumMethodEval), codegenClassScope)
                .AddParam(innerType, "param");
            var refEps = exprSymbol.GetAddEps(methodNode);
            var refIsNewData = exprSymbol.GetAddIsNewData(methodNode);
            var refExprEvalCtx = exprSymbol.GetAddExprEvalCtx(methodNode);
            var forgeMember = codegenClassScope.AddDefaultFieldUnshared(
                true,
                typeof(object),
                NewInstance(typeof(object)));
            var block = methodNode.Block;
            var collectionType = innerType;
            if (innerType == typeof(EventBean)) {
                block.DeclareVar<ICollection<EventBean>>(
                    "coll",
                    StaticMethod(typeof(Collections), "SingletonList", new[] { typeof(EventBean) }, Ref("param")));
                collectionType = typeof(ICollection<EventBean>);
            }
            else if (innerType == typeof(object[])) {
                block.DeclareVar<ICollection<object>>("coll", Unwrap<object>(Ref("param")));
                collectionType = typeof(ICollection<object>);
            }
            else if (innerType.IsGenericCollection()) {
                block.DeclareVar(innerType, "coll", Ref("param"));
            }
            else {
                throw new IllegalStateException("invalid type presented for unwrapping");
            }

            block.DeclareVar<ExpressionResultCacheForEnumerationMethod>(
                "cache",
                ExprDotMethodChain(refExprEvalCtx)
                    .Get("ExpressionResultCacheService")
                    .Get("AllocateEnumerationMethod"));

            var premade = new EnumForgeCodegenParams(
                Ref("eventsLambda"),
                Ref("coll"),
                collectionType,
                refIsNewData,
                refExprEvalCtx,
                returnType);
            
            if (forge.IsCache) {
                block.DeclareVar<ExpressionResultCacheEntryLongArrayAndObj>(
                        "cacheValue",
                        ExprDotMethod(Ref("cache"), "GetEnumerationMethodLastValue", forgeMember))
                    .IfCondition(NotEqualsNull(Ref("cacheValue")))
                    .BlockReturn(Cast(returnType, ExprDotName(Ref("cacheValue"), "Result")))
                    .IfRefNullReturnNull("coll")
                    .DeclareVar<EventBean[]>(
                        "eventsLambda",
                        StaticMethod(
                            typeof(ExprDotForgeEnumMethodEval),
                            "AllocateCopyEventLambda",
                            refEps,
                            Constant(forge.EnumEvalNumRequiredEvents)))
                    .DeclareVar(
                        forge.TypeInfo.GetCodegenReturnType(),
                        "result",
                        forge.EnumForge.Codegen(premade, methodNode, codegenClassScope))
                    .Expression(
                        ExprDotMethod(Ref("cache"), "SaveEnumerationMethodLastValue", forgeMember, Ref("result")))
                    .MethodReturn(Ref("result"));
            }
            else {
                var contextNumberMember = codegenClassScope.AddDefaultFieldUnshared(
                    true,
                    typeof(AtomicLong),
                    NewInstance(typeof(AtomicLong)));

                var returnInvocation = forge.EnumForge.Codegen(premade, methodNode, codegenClassScope);
                
                block
                    .CommentFullLine(MethodBase.GetCurrentMethod()!.DeclaringType!.FullName + "." + MethodBase.GetCurrentMethod()!.Name)
                    .DeclareVar<long>("contextNumber", ExprDotMethod(contextNumberMember, "GetAndIncrement"))
                    .TryCatch()
                    .Expression(ExprDotMethod(Ref("cache"), "PushContext", Ref("contextNumber")))
                    .IfRefNullReturnNull("coll")
                    .DeclareVar<EventBean[]>(
                        "eventsLambda",
                        StaticMethod(
                            typeof(ExprDotForgeEnumMethodEval),
                            "AllocateCopyEventLambda",
                            refEps,
                            Constant(forge.EnumEvalNumRequiredEvents)))
                    //.TryReturn(CodegenLegoCast.CastSafeFromObjectType(returnType, returnInvocation))
                    .TryReturn(returnInvocation)
                    .TryFinally()
                    .Expression(ExprDotMethod(Ref("cache"), "PopContext"))
                    .BlockEnd()
                    .MethodEnd();
            }

            return LocalMethod(methodNode, inner);
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name = "eventsPerStream">events</param>
        /// <param name = "enumEvalNumRequiredEvents">width</param>
        /// <returns>allocated</returns>
        public static EventBean[] AllocateCopyEventLambda(
            EventBean[] eventsPerStream,
            int enumEvalNumRequiredEvents)
        {
            var eventsLambda = new EventBean[enumEvalNumRequiredEvents];
            EventBeanUtility.SafeArrayCopy(eventsPerStream, eventsLambda);
            return eventsLambda;
        }

        public EPChainableType TypeInfo => _forge.TypeInfo;

        public ExprDotForge DotForge => _forge;
    }
} // end of namespace