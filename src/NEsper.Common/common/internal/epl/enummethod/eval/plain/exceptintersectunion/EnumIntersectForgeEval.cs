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
using com.espertech.esper.common.client.collection;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.enummethod.codegen;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval.plain.exceptintersectunion
{
    public class EnumIntersectForgeEval : EnumEval
    {
        private readonly EnumIntersectForge _forge;
        private readonly ExprEnumerationEval _evaluator;

        public EnumIntersectForgeEval(
            EnumIntersectForge forge,
            ExprEnumerationEval evaluator)
        {
            _forge = forge;
            _evaluator = evaluator;
        }

        public object EvaluateEnumMethod(
            EventBean[] eventsLambda,
            ICollection<object> enumcoll,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            if (_forge.scalar) {
                var other = _evaluator.EvaluateGetROCollectionScalar(eventsLambda, isNewData, context);
                return EnumIntersectForgeEvalSet(other, enumcoll);
            }
            else {
                var other = _evaluator.EvaluateGetROCollectionEvents(eventsLambda, isNewData, context);
                return EnumIntersectForgeEvalSet(other, enumcoll.Unwrap<EventBean>());
            }
        }

        public static CodegenExpression Codegen(
            EnumIntersectForge forge,
            EnumForgeCodegenParams args,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            var scope = new ExprForgeCodegenSymbol(false, null);

            var methodNode = codegenMethodScope
                .MakeChildWithScope(
                    args.EnumcollType,
                    typeof(EnumIntersectForgeEval),
                    scope,
                    codegenClassScope)
                .AddParam(ExprForgeCodegenNames.FP_EPS)
                .AddParam(args.EnumcollType, EnumForgeCodegenNames.REF_ENUMCOLL.Ref)
                .AddParam(ExprForgeCodegenNames.FP_ISNEWDATA)
                .AddParam(ExprForgeCodegenNames.FP_EXPREVALCONTEXT);

            var block = methodNode.Block;
            if (forge.scalar) {
                block.DeclareVar(
                    args.EnumcollType,
                    "other",
                    forge.evaluatorForge.EvaluateGetROCollectionScalarCodegen(methodNode, scope, codegenClassScope));
            }
            else {
                block.DeclareVar(
                    args.EnumcollType,
                    "other",
                    forge.evaluatorForge.EvaluateGetROCollectionEventsCodegen(methodNode, scope, codegenClassScope));
            }

            block.MethodReturn(
                StaticMethod(
                    typeof(EnumIntersectForgeEval),
                    "EnumIntersectForgeEvalSet",
                    Ref("other"),
                    EnumForgeCodegenNames.REF_ENUMCOLL));

            return LocalMethod(methodNode, args.Eps, args.Enumcoll, args.IsNewData, args.ExprCtx);
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="other">other</param>
        /// <param name="enumcoll">coll</param>
        /// <returns>intersection</returns>
        public static ICollection<T> EnumIntersectForgeEvalSet<T>(
            ICollection<T> other,
            ICollection<T> enumcoll)
        {
            if (other == null || other.IsEmpty() || enumcoll.IsEmpty()) {
                return enumcoll.Unwrap<T>();
            }

            var resultX = new List<T>(enumcoll);
            resultX.RetainAll(other.Unwrap<T>());
            return resultX;
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="other">other</param>
        /// <param name="enumcoll">coll</param>
        /// <returns>intersection</returns>
        public static ICollection<EventBean> EnumIntersectForgeEvalSet(
            ICollection<EventBean> other,
            ICollection<EventBean> enumcoll)
        {
            if (other == null || other.IsEmpty() || enumcoll.IsEmpty()) {
                return enumcoll;
            }

            var targetEvents = enumcoll.Unwrap<EventBean>();
            var sourceEvents = other.Unwrap<EventBean>();
            var result = new List<EventBean>();

            // we compare event underlying
            foreach (var targetEvent in targetEvents) {
                if (targetEvent == null) {
                    result.Add(null);
                    continue;
                }

                var found = false;
                foreach (var sourceEvent in sourceEvents) {
                    if (targetEvent == sourceEvent) {
                        found = true;
                        break;
                    }

                    if (sourceEvent == null) {
                        continue;
                    }

                    if (targetEvent.Underlying.Equals(sourceEvent.Underlying)) {
                        found = true;
                        break;
                    }
                }

                if (found) {
                    result.Add(targetEvent);
                }
            }

            return result;
        }
    }
} // end of namespace