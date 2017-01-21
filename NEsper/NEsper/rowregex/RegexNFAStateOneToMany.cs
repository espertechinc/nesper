///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;

namespace com.espertech.esper.rowregex
{
    /// <summary>
    /// The '+' state in the regex NFA states.
    /// </summary>
    public class RegexNFAStateOneToMany
        : RegexNFAStateBase
        , RegexNFAState
    {
        private readonly ExprEvaluator _exprNode;
        private readonly bool _exprRequiresMultimatchState;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="nodeNum">node num</param>
        /// <param name="variableName">variable name</param>
        /// <param name="streamNum">stream number</param>
        /// <param name="multiple">true for multiple matches</param>
        /// <param name="isGreedy">true for greedy</param>
        /// <param name="exprNode">filter expression</param>
        /// <param name="exprRequiresMultimatchState">if set to <c>true</c> [expr requires multimatch state].</param>
        public RegexNFAStateOneToMany(String nodeNum, String variableName, int streamNum, bool multiple, bool? isGreedy, ExprNode exprNode, bool exprRequiresMultimatchState)
            : base(nodeNum, variableName, streamNum, multiple, isGreedy)
        {
            _exprNode = exprNode == null ? null : exprNode.ExprEvaluator;
            _exprRequiresMultimatchState = exprRequiresMultimatchState;
            AddState(this);
        }
    
        public override bool Matches(EventBean[] eventsPerStream, ExprEvaluatorContext exprEvaluatorContext)
        {
            if (_exprNode == null)
            {
                return true;
            }
            var result = (bool?) _exprNode.Evaluate(new EvaluateParams(eventsPerStream, true, exprEvaluatorContext));
            if (result != null)
            {
                return result.Value;
            }
            return false;
        }
    
        public override String ToString()
        {
            if (_exprNode == null)
            {
                return "OneMany-Unfiltered";
            }
            return "OneMany-Filtered";
        }

        public override bool IsExprRequiresMultimatchState
        {
            get { return _exprRequiresMultimatchState; }
        }
    }
}
