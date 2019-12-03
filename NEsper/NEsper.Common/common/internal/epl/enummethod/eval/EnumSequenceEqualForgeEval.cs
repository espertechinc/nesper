///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.enummethod.codegen;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval
{
    public class EnumSequenceEqualForgeEval : EnumEval
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ExprEvaluator _innerExpression;

        public EnumSequenceEqualForgeEval(ExprEvaluator innerExpression)
        {
            _innerExpression = innerExpression;
        }

        public object EvaluateEnumMethod(
            EventBean[] eventsLambda,
            ICollection<object> enumcoll,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            var otherObj = _innerExpression.Evaluate(eventsLambda, isNewData, context);
            return EnumSequenceEqualsCompare(enumcoll, otherObj);
        }

        public static CodegenExpression Codegen(
            EnumSequenceEqualForge forge,
            EnumForgeCodegenParams args,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            var scope = new ExprForgeCodegenSymbol(false, null);
            var namedParams = forge.Scalar
                ? EnumForgeCodegenNames.PARAMS_OBJECT
                : EnumForgeCodegenNames.PARAMS_EVENTBEAN;

            var methodNode = codegenMethodScope
                .MakeChildWithScope(typeof(bool), typeof(EnumSequenceEqualForgeEval), scope, codegenClassScope)
                .AddParam(namedParams);

            methodNode.Block.MethodReturn(
                StaticMethod(
                    typeof(EnumSequenceEqualForgeEval),
                    "EnumSequenceEqualsCompare",
                    EnumForgeCodegenNames.REF_ENUMCOLL,
                    forge.InnerExpression.EvaluateCodegen(typeof(object), methodNode, scope, codegenClassScope)));
            return LocalMethod(
                methodNode,
                args.Eps,
                args.Enumcoll,
                args.IsNewData,
                args.ExprCtx);
        }

        public static bool EnumSequenceEqualsCompare<T>(
            ICollection<T> enumcoll,
            object otherObj)
        {
            if (otherObj == null) {
                return false;
            }

            if (otherObj is ICollection<T> otherCollection) {
                if (enumcoll.Count != otherCollection.Count) {
                    return false;
                }

                if (enumcoll.IsEmpty()) {
                    return true;
                }

                IEnumerator oneEnum = enumcoll.GetEnumerator();
                IEnumerator twoEnum = otherCollection.GetEnumerator();
                return CompareSequenceImpl<T>(oneEnum, twoEnum);
            }

            if (otherObj is Array otherArray) {
                if (enumcoll.Count != otherArray.Length) {
                    return false;
                }

                if (enumcoll.IsEmpty()) {
                    return true;
                }

                IEnumerator oneEnum = enumcoll.GetEnumerator();
                IEnumerator twoEnum = otherArray.GetEnumerator();
                return CompareSequenceImpl<T>(oneEnum, twoEnum);
            }

            var otherObjType = otherObj.GetType();
            if (otherObjType.IsGenericCollection() && typeof(T).IsAssignableFrom(otherObjType.GetCollectionItemType())) {
                var unwrapCollection = otherObj.Unwrap<T>();
                if (enumcoll.Count != unwrapCollection.Count) {
                    return false;
                }

                if (enumcoll.IsEmpty()) {
                    return true;
                }

                IEnumerator oneEnum = enumcoll.GetEnumerator();
                IEnumerator twoEnum = unwrapCollection.GetEnumerator();
                return CompareSequenceImpl<T>(oneEnum, twoEnum);
            }

            Log.Warn(
                "Enumeration method 'sequenceEqual' expected a Collection-type return value from its parameter but received '" +
                otherObj.GetType().CleanName() + "'");
            return false;
        }

        private static bool CompareSequenceImpl<T>(IEnumerator oneEnum,
            IEnumerator twoEnum)
        {
            while (oneEnum.MoveNext() && twoEnum.MoveNext()) {
                var first = oneEnum.Current;
                var second = twoEnum.Current;
                if (!Equals(first, second)) {
                    return false;
                }
            }

            return true;
        }
    }
} // end of namespace