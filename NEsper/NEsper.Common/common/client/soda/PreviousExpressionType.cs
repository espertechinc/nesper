///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.client.soda
{
    /// <summary>
    /// Previous function type.
    /// </summary>
    public enum PreviousExpressionType
    {
        /// <summary>
        /// Returns a previous event with the index counting from the last event
        /// towards the first event.
        /// </summary>
        PREV,

        /// <summary>
        /// Returns the count of previous events.
        /// </summary>
        PREVCOUNT,

        /// <summary>
        /// Returns a previous event with the index counting from the first event
        /// towards the last event.
        /// </summary>
        PREVTAIL,

        /// <summary>
        /// Returns all previous events.
        /// </summary>
        PREVWINDOW
    }
}