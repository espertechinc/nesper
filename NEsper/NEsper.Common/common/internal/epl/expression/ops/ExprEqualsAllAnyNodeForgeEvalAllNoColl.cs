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
    public class ExprEqualsAllAnyNodeForgeEvalAllNoColl : ExprEvaluator
    {
        private readonly ExprEvaluator[] evaluators;
        private readonly ExprEqualsAllAnyNodeForge forge;

        public ExprEqualsAllAnyNodeForgeEvalAllNoColl(
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

            return CompareAll(
                forge.ForgeRenderable.IsNot, leftResult, eventsPerStream, isNewData, exprEvaluatorContext);
        }

        private object CompareAll(
            bool isNot,
            object leftResult,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var equalsMeansContinue = !isNot;
            var len = evaluators.Length - 1;
            if (len > 0 && leftResult == null) {
                return null;
            }

            var hasNonNullRow = false;
            var hasNullRow = false;
            for (var i = 1; i <= len; i++) {
                var rightResult = evaluators[i].Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);

                if (rightResult != null) {
                    hasNonNullRow = true;
                    if (!forge.IsMustCoerce) {
                        if (equalsMeansContinue ^ leftResult.Equals(rightResult)) {
                            return false;
                        }
                    }
                    else {
                        var right = forge.Coercer.CoerceBoxed(rightResult);
                        if (equalsMeansContinue ^ leftResult.Equals(right)) {
                            return false;
                        }
                    }
                }
                else {
                    hasNullRow = true;
                }
            }

            if (!hasNonNullRow || hasNullRow) {
                return null;
            }

            return true;
        }
    }
} // end of namespace