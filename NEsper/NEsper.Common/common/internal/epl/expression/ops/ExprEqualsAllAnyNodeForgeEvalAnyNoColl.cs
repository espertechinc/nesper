///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.epl.expression.ops
{
    public class ExprEqualsAllAnyNodeForgeEvalAnyNoColl : ExprEvaluator
    {
        private readonly ExprEvaluator[] evaluators;
        private readonly ExprEqualsAllAnyNodeForge forge;

        public ExprEqualsAllAnyNodeForgeEvalAnyNoColl(
            ExprEqualsAllAnyNodeForge forge,
            ExprEvaluator[] evaluators)
        {
            this.forge = forge;
            this.evaluators = evaluators;
        }

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var result = EvaluateInternal(eventsPerStream, isNewData, exprEvaluatorContext);
            return result;
        }

        private object EvaluateInternal(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var leftResult = evaluators[0].Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);

            // coerce early if testing without collections
            if (forge.IsMustCoerce && leftResult != null) {
                leftResult = forge.Coercer.CoerceBoxed(leftResult);
            }

            return CompareAny(leftResult, eventsPerStream, isNewData, exprEvaluatorContext);
        }

        private object CompareAny(
            object leftResult,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var isNot = forge.ForgeRenderable.IsNot;
            var hasNonNullRow = false;
            var hasNullRow = false;
            var len = forge.ForgeRenderable.ChildNodes.Length - 1;
            for (var i = 1; i <= len; i++) {
                var rightResult = evaluators[i].Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);

                if (leftResult == null) {
                    return null;
                }

                if (rightResult == null) {
                    hasNullRow = true;
                    continue;
                }

                hasNonNullRow = true;
                if (!forge.IsMustCoerce) {
                    if (!isNot && leftResult.Equals(rightResult) || isNot && !leftResult.Equals(rightResult)) {
                        return true;
                    }
                }
                else {
                    var right = forge.Coercer.CoerceBoxed(rightResult);
                    if (!isNot && leftResult.Equals(right) || isNot && !leftResult.Equals(right)) {
                        return true;
                    }
                }
            }

            if (!hasNonNullRow || hasNullRow) {
                return null;
            }

            return false;
        }
    }
} // end of namespace