///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.client.util
{
    /// <summary>
    ///     Choice for type of pattern object.
    /// </summary>
    public enum PatternObjectType
    {
        /// <summary>
        ///     Observer observes externally-supplied events.
        /// </summary>
        OBSERVER,

        /// <summary>
        ///     Guard allows or disallows events from child expressions to pass.
        /// </summary>
        GUARD
    }
} // end of namespace