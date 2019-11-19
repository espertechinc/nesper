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
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.enummethod.eval
{
    public class EnumExceptForgeEval : EnumEval
    {
        private readonly ExprEnumerationEval evaluator;
        private readonly EnumExceptForge forge;

        public EnumExceptForgeEval(
            EnumExceptForge forge,
            ExprEnumerationEval evaluator)
        {
            this.forge = forge;
            this.evaluator = evaluator;
        }

        public object EvaluateEnumMethod(
            EventBean[] eventsLambda,
            ICollection<object> enumcoll,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            if (enumcoll == null) {
                return null;
            }

            if (forge.scalar) {
                var set = evaluator.EvaluateGetROCollectionScalar(eventsLambda, isNewData, context);
                return EnumExceptForgeEvalSet(set, enumcoll);
            }
            else {
                var set = evaluator.EvaluateGetROCollectionEvents(eventsLambda, isNewData, context);
                return EnumExceptForgeEvalSet(set, enumcoll.Unwrap<EventBean>());
            }
        }

        public static CodegenExpression Codegen(
            EnumExceptForge forge,
            EnumForgeCodegenParams args,
            CodegenMethodScope codegenMethodScope,
            CodegenClassScope codegenClassScope)
        {
            var namedParams = forge.scalar
                ? EnumForgeCodegenNames.PARAMS_OBJECT
                : EnumForgeCodegenNames.PARAMS_EVENTBEAN;
            var returnType = forge.scalar
                ? typeof(ICollection<object>)
                : typeof(ICollection<EventBean>);

            var scope = new ExprForgeCodegenSymbol(false, null);
            var methodNode = codegenMethodScope.MakeChildWithScope(
                    returnType,
                    typeof(EnumIntersectForgeEval),
                    scope,
                    codegenClassScope)
                .AddParam(namedParams);

            var block = methodNode.Block;
            if (forge.scalar) {
                block.DeclareVar<ICollection<object>>(
                    "other",
                    forge.evaluatorForge.EvaluateGetROCollectionScalarCodegen(methodNode, scope, codegenClassScope));
            }
            else {
                block.DeclareVar<ICollection<EventBean>>(
                    "other",
                    forge.evaluatorForge.EvaluateGetROCollectionEventsCodegen(methodNode, scope, codegenClassScope));
            }

            block.MethodReturn(
                StaticMethod(
                    typeof(EnumExceptForgeEval),
                    "EnumExceptForgeEvalSet",
                    Ref("other"),
                    EnumForgeCodegenNames.REF_ENUMCOLL));
            return LocalMethod(methodNode, args.Eps, args.Enumcoll, args.IsNewData, args.ExprCtx);
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="other">other</param>
        /// <param name="enumcoll">coll</param>
        /// <returns>intersection</returns>
        public static ICollection<object> EnumExceptForgeEvalSet(
            ICollection<object> other,
            ICollection<object> enumcoll)
        {
            if (other == null || other.IsEmpty() || enumcoll.IsEmpty()) {
                return enumcoll;
            }

            var resultX = new List<object>(enumcoll);
            resultX.RemoveAll(other);
            return resultX.Unwrap<object>();
        }
        
        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="other">other</param>
        /// <param name="enumcoll">coll</param>
        /// <returns>intersection</returns>
        public static ICollection<EventBean> EnumExceptForgeEvalSet(
            ICollection<EventBean> other,
            ICollection<EventBean> enumcoll)
        {
            if (other == null || other.IsEmpty() || enumcoll.IsEmpty()) {
                return enumcoll.Unwrap<EventBean>();
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

                if (!found) {
                    result.Add(targetEvent);
                }
            }

            return result;
        }
    }
} // end of namespace