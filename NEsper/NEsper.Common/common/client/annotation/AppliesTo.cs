///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.common.client.annotation
{
    /// <summary>Annotation to target certain constructs.</summary>
    public enum AppliesTo
    {
        /// <summary>
        /// Undefined
        /// </summary>
        UNDEFINED,
        /// <summary>
        /// Unique-view
        /// </summary>
        UNIQUE,
        /// <summary>
        /// Group-by
        /// </summary>
        GROUPBY,
        /// <summary>
        /// Index
        /// </summary>
        INDEX,
        /// <summary>
        /// Output rate limiting.
        /// </summary>
        OUTPUTLIMIT,
        /// <summary>
        /// Match-recognize
        /// </summary>
        MATCHRECOGNIZE,
        /// <summary>
        /// Contexts
        /// </summary>
        CONTEXT,
        /// <summary>
        /// Prior window.
        /// </summary>
        PRIOR,
        /// <summary>
        /// Rank window.
        /// </summary>
        RANK,
        /// <summary>
        /// Every-distinct pattern.
        /// </summary>
        EVERYDISTINCT,
        /// <summary>
        /// Sorted window.
        /// </summary>
        SORTEDWIN,
        /// <summary>
        /// Time-order window.
        /// </summary>
        TIMEORDERWIN,
        /// <summary>
        /// Keep-all window.
        /// </summary>
        KEEPALLWIN,
        /// <summary>
        /// Pattern
        /// </summary>
        PATTERN,
        /// <summary>
        /// Time-accumulative window.
        /// </summary>
        TIMEACCUMWIN,
        /// <summary>
        /// Time-batch window.
        /// </summary>
        TIMEBATCHWIN,
        /// <summary>
        /// Time-length batch window.
        /// </summary>
        TIMELENGTHBATCHWIN,
        /// <summary>
        /// Grouped window.
        /// </summary>
        GROUPWIN,
        /// <summary>
        /// Length-window.
        /// </summary>
        LENGTHWIN,
        /// <summary>
        /// Time-window.
        /// </summary>
        TIMEWIN,
        /// <summary>
        /// Length-batch window.
        /// </summary>
        LENGTHBATCHWIN,
        /// <summary>
        /// Previous functions.
        /// </summary>
        PREV,
        /// <summary>
        /// Expression window
        /// </summary>
        EXPRESSIONWIN,
        /// <summary>
        /// Expression batch window.
        /// </summary>
        EXPRESSIONBATCHWIN,
        /// <summary>
        /// Pattern followed-by.
        /// </summary>
        FOLLOWEDBY,
        /// <summary>
        /// First-length window.
        /// </summary>
        FIRSTLENGTHWIN,
        /// <summary>
        /// Externally-timed window.
        /// </summary>
        EXTTIMEDWIN,
        /// <summary>
        /// Externally-timed batch window.
        /// </summary>
        EXTTIMEDBATCHWIN
    }
} // end of namespace