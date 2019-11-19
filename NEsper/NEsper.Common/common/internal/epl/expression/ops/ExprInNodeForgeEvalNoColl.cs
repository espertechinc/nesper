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
    /// <summary>
    ///     Represents the in-clause (set check) function in an expression tree.
    /// </summary>
    public class ExprInNodeForgeEvalNoColl : ExprEvaluator
    {
        private readonly ExprEvaluator[] evaluators;
        private readonly ExprInNodeForge forge;

        public ExprInNodeForgeEvalNoColl(
            ExprInNodeForge forge,
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

        private bool? EvaluateInternal(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var inPropResult = evaluators[0].Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);

            var isNotIn = forge.ForgeRenderable.IsNotIn;
            if (forge.IsMustCoerce && inPropResult != null) {
                inPropResult = forge.Coercer.CoerceBoxed(inPropResult);
            }

            var len = evaluators.Length - 1;
            if (len > 0 && inPropResult == null) {
                return null;
            }

            var hasNullRow = false;
            for (var i = 1; i <= len; i++) {
                var rightResult = evaluators[i].Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);

                if (rightResult == null) {
                    hasNullRow = true;
                    continue;
                }

                if (!forge.IsMustCoerce) {
                    if (rightResult.Equals(inPropResult)) {
                        return !isNotIn;
                    }
                }
                else {
                    var right = forge.Coercer.CoerceBoxed(rightResult);
                    if (right.Equals(inPropResult)) {
                        return !isNotIn;
                    }
                }
            }

            if (hasNullRow) {
                return null;
            }

            return isNotIn;
        }
    }
} // end of namespace