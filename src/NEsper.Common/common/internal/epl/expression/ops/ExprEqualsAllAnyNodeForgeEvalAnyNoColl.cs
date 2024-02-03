///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
        private readonly ExprEvaluator[] _evaluators;
        private readonly ExprEqualsAllAnyNodeForge _forge;

        public ExprEqualsAllAnyNodeForgeEvalAnyNoColl(
            ExprEqualsAllAnyNodeForge forge,
            ExprEvaluator[] evaluators)
        {
            _forge = forge;
            _evaluators = evaluators;
        }

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            return EvaluateInternal(eventsPerStream, isNewData, exprEvaluatorContext);
        }

        private object EvaluateInternal(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var leftResult = _evaluators[0].Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);

            // coerce early if testing without collections
            if (_forge.IsMustCoerce && leftResult != null) {
                leftResult = _forge.Coercer.CoerceBoxed(leftResult);
            }

            return CompareAny(leftResult, eventsPerStream, isNewData, exprEvaluatorContext);
        }

        private object CompareAny(
            object leftResult,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var isNot = _forge.ForgeRenderable.IsNot;
            var hasNonNullRow = false;
            var hasNullRow = false;
            var len = _forge.ForgeRenderable.ChildNodes.Length - 1;
            for (var i = 1; i <= len; i++) {
                var rightResult = _evaluators[i].Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);

                if (leftResult == null) {
                    return null;
                }

                if (rightResult == null) {
                    hasNullRow = true;
                    continue;
                }

                hasNonNullRow = true;
                if (!_forge.IsMustCoerce) {
                    if ((!isNot && leftResult.Equals(rightResult)) || (isNot && !leftResult.Equals(rightResult))) {
                        return true;
                    }
                }
                else {
                    var right = _forge.Coercer.CoerceBoxed(rightResult);
                    if ((!isNot && leftResult.Equals(right)) || (isNot && !leftResult.Equals(right))) {
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