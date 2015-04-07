///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
    /// Any-quantifier.
    /// </summary>
    public class RegexNFAStateAnyOne
        : RegexNFAStateBase
        , RegexNFAState
    {
        /// <summary>Ctor. </summary>
        /// <param name="nodeNum">node num</param>
        /// <param name="variableName">variable</param>
        /// <param name="streamNum">stream num</param>
        /// <param name="multiple">indicator</param>
        public RegexNFAStateAnyOne(String nodeNum, String variableName, int streamNum, bool multiple)

                    : base(nodeNum, variableName, streamNum, multiple, null)
        {
        }
    
        public override bool Matches(EventBean[] eventsPerStream, ExprEvaluatorContext exprEvaluatorContext)
        {
            return true;
        }
    
        public override String ToString()
        {
            return "AnyEvent";
        }

        public override bool IsExprRequiresMultimatchState
        {
            get { return false; }
        }
    }
}
