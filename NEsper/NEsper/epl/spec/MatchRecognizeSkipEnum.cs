///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.epl.spec
{
    /// <summary>Skip-enum for match_recognize. </summary>
    public enum MatchRecognizeSkipEnum
    {
        /// <summary>Skip to current row. </summary>
        TO_CURRENT_ROW,

        /// <summary>Skip to next row. </summary>
        TO_NEXT_ROW,

        /// <summary>Skip past last row. </summary>
        PAST_LAST_ROW
    }
}