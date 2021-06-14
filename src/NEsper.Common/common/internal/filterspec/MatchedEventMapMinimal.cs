///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.filterspec
{
    public interface MatchedEventMapMinimal
    {
        /// <summary>
        ///     Returns a map containing the events where the key is the event tag string and the value is the event
        ///     instance.
        /// </summary>
        /// <returns>Map containing event instances</returns>
        object[] MatchingEvents { get; }

        MatchedEventMapMeta Meta { get; }
    }
} // end of namespace