///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace com.espertech.esper.epl.spec
{
    /// <summary>
    /// Specification for the merge statement.
    /// </summary>
    public class OnTriggerMergeDesc : OnTriggerWindowDesc
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OnTriggerMergeDesc"/> class.
        /// </summary>
        /// <param name="windowName">Name of the window.</param>
        /// <param name="optionalAsName">Name of the optional as.</param>
        /// <param name="items">The items.</param>
        public OnTriggerMergeDesc(String windowName, String optionalAsName, IList<OnTriggerMergeMatched> items)
            : base(windowName, optionalAsName, OnTriggerType.ON_MERGE, false)
        {
            Items = items;
        }

        public IList<OnTriggerMergeMatched> Items { get; private set; }
    }
    
}
