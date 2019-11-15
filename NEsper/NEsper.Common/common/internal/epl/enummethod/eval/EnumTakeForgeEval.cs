///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.enummethod.codegen;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval
{
    public class EnumTakeForgeEval : EnumEval
    {
        private readonly ExprEvaluator sizeEval;

        public EnumTakeForgeEval(ExprEvaluator sizeEval)
        {
            this.sizeEval = sizeEval;
        }

        public object EvaluateEnumMethod(
            EventBean[] eventsLambda,
            ICollection<object> enumcoll,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            var size = sizeEval.Evaluate(eventsLambda, isNewData, context);
            if (size == null) {
                return null;
            }

            return EvaluateEnumTakeMethod(enumcoll, size.AsInt());
        }

        public static CodegenExpression Codegen(
            EnumTakeForge forge,
            EnumForgeCodegenParams args,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            var isEventBeanCollection = !forge.Scalar;
            var scope = new ExprForgeCodegenSymbol(false, null);
            var returnType = isEventBeanCollection
                ? typeof(ICollection<EventBean>)
                : typeof(ICollection<object>);
            var paramTypes = isEventBeanCollection
                ? EnumForgeCodegenNames.PARAMS_EVENTBEAN
                : EnumForgeCodegenNames.PARAMS_OBJECT;
            var methodNode = codegenMethodScope.MakeChildWithScope(
                    returnType,
                    typeof(EnumTakeForgeEval),
                    scope,
                    codegenClassScope)
                .AddParam(paramTypes);

            var sizeType = forge.SizeEval.EvaluationType;
            var block = methodNode.Block
                .DebugStack()
                .DeclareVar(
                    sizeType,
                    "size",
                    forge.SizeEval.EvaluateCodegen(sizeType, methodNode, scope, codegenClassScope));
            if (!sizeType.IsPrimitive) {
                block.IfRefNullReturnNull("size");
            }

            block.MethodReturn(
                StaticMethod(
                    typeof(EnumTakeForgeEval),
                    "EvaluateEnumTakeMethod",
                    EnumForgeCodegenNames.REF_ENUMCOLL,
                    SimpleNumberCoercerFactory.CoercerInt.CodegenInt(Ref("size"), sizeType)));
            return LocalMethod(methodNode, args.Eps, args.Enumcoll, args.IsNewData, args.ExprCtx);
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="enumcoll">collection</param>
        /// <param name="size">size</param>
        /// <returns>collection</returns>
        public static ICollection<T> EvaluateEnumTakeMethod<T>(
            ICollection<T> enumcoll,
            int size)
        {
            if (CompatExtensions.IsEmpty(enumcoll)) {
                return enumcoll;
            }

            if (size <= 0) {
                return Collections.GetEmptyList<T>();
            }

            if (enumcoll.Count < size) {
                return enumcoll;
            }

            if (size == 1) {
                return Collections.SingletonList(enumcoll.First());
            }

            var result = new List<T>(size);
            foreach (var next in enumcoll) {
                if (result.Count >= size) {
                    break;
                }

                result.Add(next);
            }

            return result;
        }
    }
} // end of namespace