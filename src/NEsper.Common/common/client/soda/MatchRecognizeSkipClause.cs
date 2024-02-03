///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.client.soda
{
    /// <summary>Skip clause enum for match recognize. </summary>
    public enum MatchRecognizeSkipClause
    {
        /// <summary>Skip to current row. </summary>
        TO_CURRENT_ROW,

        /// <summary>Skip to next row. </summary>
        TO_NEXT_ROW,

        /// <summary>Skip past last row. </summary>
        PAST_LAST_ROW
    }

    public static class MatchRecognizeSkipClauseExtensions
    {
        /// <summary>Returns clause text. </summary>
        /// <returns>textual</returns>
        public static string GetText(this MatchRecognizeSkipClause value)
        {
            switch (value) {
                case MatchRecognizeSkipClause.TO_CURRENT_ROW:
                    return "to current row";

                case MatchRecognizeSkipClause.TO_NEXT_ROW:
                    return "to next row";

                case MatchRecognizeSkipClause.PAST_LAST_ROW:
                    return "past last row";
            }

            throw new ArgumentException("vanameof(value)value");
        }
    }
}