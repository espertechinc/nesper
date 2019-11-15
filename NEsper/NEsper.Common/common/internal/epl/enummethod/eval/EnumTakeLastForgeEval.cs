///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

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
    public class EnumTakeLastForgeEval : EnumEval
    {
        private readonly ExprEvaluator sizeEval;

        public EnumTakeLastForgeEval(ExprEvaluator sizeEval)
        {
            this.sizeEval = sizeEval;
        }

        public object EvaluateEnumMethod(
            EventBean[] eventsLambda,
            ICollection<object> enumcoll,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            var sizeObj = sizeEval.Evaluate(eventsLambda, isNewData, context);
            if (sizeObj == null) {
                return null;
            }

            return EvaluateEnumMethodTakeLast(enumcoll, sizeObj.AsInt());
        }

        public static CodegenExpression Codegen(
            EnumTakeLastForge forge,
            EnumForgeCodegenParams args,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            var sizeType = forge.sizeEval.EvaluationType;

            var returnType = args.EnumcollType ?? typeof(ICollection<EventBean>);
            var paramArgs = returnType == typeof(ICollection<EventBean>)
                ? EnumForgeCodegenNames.PARAMS_EVENTBEAN
                : EnumForgeCodegenNames.PARAMS_OBJECT;

            var scope = new ExprForgeCodegenSymbol(false, null);
            var methodNode = codegenMethodScope.MakeChildWithScope(
                    returnType,
                    typeof(EnumTakeLastForgeEval),
                    scope,
                    codegenClassScope)
                .AddParam(paramArgs);

            var block = methodNode.Block.DeclareVar(
                sizeType,
                "size",
                forge.sizeEval.EvaluateCodegen(sizeType, methodNode, scope, codegenClassScope));
            if (!sizeType.IsPrimitive) {
                block.IfRefNullReturnNull("size");
            }

            block.MethodReturn(
                StaticMethod(
                    typeof(EnumTakeLastForgeEval),
                    "EvaluateEnumMethodTakeLast",
                    EnumForgeCodegenNames.REF_ENUMCOLL,
                    SimpleNumberCoercerFactory.CoercerInt.CodegenInt(Ref("size"), sizeType)));
            return LocalMethod(methodNode, args.Eps, args.Enumcoll, args.IsNewData, args.ExprCtx);
        }

        public static ICollection<T> EvaluateEnumMethodTakeLast<T>(
            ICollection<T> enumcoll,
            int size)
        {
            if (enumcoll.IsEmpty()) {
                return enumcoll;
            }

            if (size <= 0) {
                return Collections.GetEmptyList<T>();
            }

            if (enumcoll.Count < size) {
                return enumcoll;
            }

            if (size == 1) {
                var last = default(T);
                foreach (var next in enumcoll) {
                    last = next;
                }

                return Collections.SingletonList<T>(last);
            }

            var result = new List<T>();
            foreach (var next in enumcoll) {
                result.Add(next);
                if (result.Count > size) {
                    result.RemoveAt(0);
                }
            }

            return result;
        }
    }
} // end of namespace