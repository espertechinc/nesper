///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.client.soda
{
    /// <summary>Interval used within match recognize. </summary>
    [Serializable]
    public class MatchRecognizeIntervalClause
    {
        /// <summary>Ctor. </summary>
        public MatchRecognizeIntervalClause()
        {
        }

        /// <summary>Ctor. </summary>
        /// <param name="expression">interval expression</param>
        /// <param name="orTerminated">indicator whether or-terminated</param>
        public MatchRecognizeIntervalClause(TimePeriodExpression expression, bool orTerminated)
        {
            Expression = expression;
            IsOrTerminated = orTerminated;
        }

        /// <summary>Returns the interval expression. </summary>
        /// <value>expression</value>
        public Expression Expression { get; set; }

        /// <summary>Returns indicator whether or-terminated is set </summary>
        /// <value>indicator</value>
        public bool IsOrTerminated { get; set; }
    }
}
