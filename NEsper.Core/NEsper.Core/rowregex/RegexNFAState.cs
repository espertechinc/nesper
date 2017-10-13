///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;

namespace com.espertech.esper.rowregex
{
    /// <summary>
    /// Match-recognize NFA states provides this information.
    /// </summary>
    public interface RegexNFAState
    {
        /// <summary>
        /// For multiple-quantifiers.
        /// </summary>
        /// <value>indicator</value>
        bool IsMultiple { get; }

        /// <summary>
        /// Returns the nested node number.
        /// </summary>
        /// <value>num</value>
        string NodeNumNested { get; }

        /// <summary>
        /// Returns the absolute node num.
        /// </summary>
        /// <value>num</value>
        int NodeNumFlat { get; }

        /// <summary>
        /// Returns the variable name.
        /// </summary>
        /// <value>name</value>
        string VariableName { get; }

        /// <summary>
        /// Returns stream number.
        /// </summary>
        /// <value>stream num</value>
        int StreamNum { get; }

        /// <summary>
        /// Returns greedy indicator.
        /// </summary>
        /// <value>greedy indicator</value>
        bool? IsGreedy { get; }

        /// <summary>
        /// Evaluate a match.
        /// </summary>
        /// <param name="eventsPerStream">variabele values</param>
        /// <param name="exprEvaluatorContext">expression evaluation context</param>
        /// <returns>match indicator</returns>
        bool Matches(EventBean[] eventsPerStream, ExprEvaluatorContext exprEvaluatorContext);

        /// <summary>
        /// Returns the next states.
        /// </summary>
        /// <value>states</value>
        IList<RegexNFAState> NextStates { get; }

        /// <summary>
        /// Whether or not the match-expression requires multimatch state
        /// </summary>
        /// <value>indicator</value>
        bool IsExprRequiresMultimatchState { get; }
    }
}
