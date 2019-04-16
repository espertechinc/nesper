///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.client
{
    /// <summary>
    /// Indicates that a problem occurred looking up, assigning or creating and event type.
    /// </summary>
    [Serializable]
    public class EventTypeException : EPException
    {
        /// <summary>Ctor. </summary>
        /// <param name="message">supplies exception details</param>
        public EventTypeException(String message)
            : base(message)
        {
        }
    }
}