///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;

namespace com.espertech.esper.rowregex
{
    /// <summary>
    /// Base for states.
    /// </summary>
    public abstract class RegexNFAStateBase : RegexNFAState 
    {
        /// <summary>Ctor. </summary>
        /// <param name="nodeNum">node num</param>
        /// <param name="variableName">variable</param>
        /// <param name="streamNum">stream num</param>
        /// <param name="multiple">indicator</param>
        /// <param name="isGreedy">greedy indicator</param>
        protected RegexNFAStateBase(String nodeNum, String variableName, int streamNum, bool multiple, bool? isGreedy)
        {
            NodeNumNested = nodeNum;
            VariableName = variableName;
            StreamNum = streamNum;
            IsMultiple =  multiple;
            IsGreedy = isGreedy;
            NextStates = new List<RegexNFAState>();
        }

        /// <summary>Assign a node number. </summary>
        /// <value>flat number</value>
        public int NodeNumFlat { get; set; }

        public string NodeNumNested { get; private set; }

        public IList<RegexNFAState> NextStates { get; private set; }

        /// <summary>Add a next state. </summary>
        /// <param name="next">state to add</param>
        public void AddState(RegexNFAState next)
        {
            NextStates.Add(next);
        }

        public bool IsMultiple { get; private set; }

        public string VariableName { get; private set; }

        public int StreamNum { get; private set; }

        public bool? IsGreedy { get; private set; }

        public abstract bool Matches(EventBean[] eventsPerStream, ExprEvaluatorContext exprEvaluatorContext);
        public abstract bool IsExprRequiresMultimatchState { get; }
    }
}
