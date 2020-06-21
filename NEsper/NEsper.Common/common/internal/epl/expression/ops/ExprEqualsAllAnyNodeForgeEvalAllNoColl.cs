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
        private readonly ExprEvaluator[] _evaluators;
        private readonly ExprEqualsAllAnyNodeForge _forge;

        public ExprEqualsAllAnyNodeForgeEvalAllNoColl(
            ExprEqualsAllAnyNodeForge forge,
            ExprEvaluator[] evaluators)
        {
            this._forge = forge;
            this._evaluators = evaluators;
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

            return CompareAll(
                _forge.ForgeRenderable.IsNot,
                leftResult,
                eventsPerStream,
                isNewData,
                exprEvaluatorContext);
        }

        private object CompareAll(
            bool isNot,
            object leftResult,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var equalsMeansContinue = !isNot;
            var len = _evaluators.Length - 1;
            if (len > 0 && leftResult == null) {
                return null;
            }

            var hasNonNullRow = false;
            var hasNullRow = false;
            for (var i = 1; i <= len; i++) {
                var rightResult = _evaluators[i].Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);

                if (rightResult != null) {
                    hasNonNullRow = true;
                    if (!_forge.IsMustCoerce) {
                        if (equalsMeansContinue ^ leftResult.Equals(rightResult)) {
                            return false;
                        }
                    }
                    else {
                        var right = _forge.Coercer.CoerceBoxed(rightResult);
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