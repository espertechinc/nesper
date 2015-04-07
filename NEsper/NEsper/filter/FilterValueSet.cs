///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.IO;

using com.espertech.esper.client;

namespace com.espertech.esper.filter
{
    /// <summary>
    ///     Contains the filter criteria to sift through events. The filter criteria are the event class
    ///     to look for and a set of parameters (property names, operators and constant/range values).
    /// </summary>
    public interface FilterValueSet
    {
        /// <summary>Returns type of event to filter for. </summary>
        /// <value>event type</value>
        EventType EventType { get; }

        /// <summary>Returns list of filter parameters. </summary>
        /// <value>list of filter params</value>
        FilterValueSetParam[][] Parameters { get; }

        void AppendTo(TextWriter writer);
    }
}