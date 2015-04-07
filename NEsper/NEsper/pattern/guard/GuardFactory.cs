///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;

namespace com.espertech.esper.pattern.guard
{
    /// <summary>Interface for a factory for <seealso cref="Guard" /> instances. </summary>
    public interface GuardFactory
    {
        /// <summary>Sets the guard object parameters. </summary>
        /// <param name="guardParameters">is a list of parameters</param>
        /// <param name="convertor">for converting a</param>
        /// <throws>GuardParameterException thrown to indicate a parameter problem</throws>
        void SetGuardParameters(IList<ExprNode> guardParameters,
                                MatchedEventConvertor convertor);

        /// <summary>Constructs a guard instance. </summary>
        /// <param name="context">services for use by guard</param>
        /// <param name="beginState">the prior matching events</param>
        /// <param name="quitable">to use for indicating the guard has quit</param>
        /// <param name="stateNodeId">a node id for the state object</param>
        /// <param name="guardState">state node for guard</param>
        /// <returns>guard instance</returns>
        Guard MakeGuard(PatternAgentInstanceContext context,
                        MatchedEventMap beginState,
                        Quitable quitable,
                        EvalStateNodeNumber stateNodeId,
                        Object guardState);
    }
}
