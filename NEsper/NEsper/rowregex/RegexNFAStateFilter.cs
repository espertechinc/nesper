///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.rowregex
{
    /// <summary>
    /// NFA state for a single match that applies a filter.
    /// </summary>
    public class RegexNFAStateFilter
        : RegexNFAStateBase
        , RegexNFAState
    {
        private readonly ExprEvaluator _exprNode;
        private readonly ExprNode _expression;
        private readonly bool _exprRequiresMultimatchState;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="nodeNum">node num</param>
        /// <param name="variableName">variable name</param>
        /// <param name="streamNum">stream number</param>
        /// <param name="multiple">true for multiple matches</param>
        /// <param name="exprNode">filter expression</param>
        /// <param name="exprRequiresMultimatchState">if set to <c>true</c> [expr requires multimatch state].</param>
        public RegexNFAStateFilter(String nodeNum, String variableName, int streamNum, bool multiple, ExprNode exprNode, bool exprRequiresMultimatchState)
            : base(nodeNum, variableName, streamNum, multiple, null)
        {
            _exprNode = exprNode.ExprEvaluator;
            _expression = exprNode;
            _exprRequiresMultimatchState = exprRequiresMultimatchState;
        }
    
        public override bool Matches(EventBean[] eventsPerStream, ExprEvaluatorContext exprEvaluatorContext)
        {
            var result = new Mutable<bool>(false);

            using (Instrument.With(
                i => i.QExprBool(_expression, eventsPerStream),
                i => i.AExprBool(result.Value)))
            {
                var temp = (bool?) _exprNode.Evaluate(new EvaluateParams(eventsPerStream, true, exprEvaluatorContext));
                if (temp != null)
                {
                    return result.Value = temp.Value;
                }
             
                return result.Value = false;
            }
        }
    
        public override String ToString()
        {
            return "FilterEvent";
        }

        public override bool IsExprRequiresMultimatchState
        {
            get { return _exprRequiresMultimatchState; }
        }
    }
}
