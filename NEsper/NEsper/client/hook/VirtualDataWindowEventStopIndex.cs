///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.client.hook
{
    /// <summary>
    /// Event to indicate that for a virtual data window an exitsing index is being stopped or destroyed.
    /// </summary>
    public class VirtualDataWindowEventStopIndex : VirtualDataWindowEvent
    {
        /// <summary>Ctor. </summary>
        /// <param name="namedWindowName">named window name</param>
        /// <param name="indexName">index name</param>
        public VirtualDataWindowEventStopIndex(String namedWindowName, String indexName) {
            NamedWindowName = namedWindowName;
            IndexName = indexName;
        }

        /// <summary>Returns the index name. </summary>
        /// <value>index name</value>
        public string IndexName { get; private set; }

        /// <summary>Returns the named window name. </summary>
        /// <value>named window name</value>
        public string NamedWindowName { get; private set; }
    }
}
