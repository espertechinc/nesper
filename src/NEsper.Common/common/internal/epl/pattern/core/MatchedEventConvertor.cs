///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.filterspec;

namespace com.espertech.esper.common.@internal.epl.pattern.core
{
    /// <summary>
    ///     Converts from a map of prior matching events to a events per stream for resolution by expressions.
    /// </summary>
    /// <param name="events">pattern partial matches</param>
    public delegate EventBean[] MatchedEventConvertor(MatchedEventMap events);
} // end of namespace