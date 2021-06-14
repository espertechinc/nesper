///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace com.espertech.esper.common.@internal.compile.stage1.spec
{
    /// <summary>
    ///     Specification for the merge statement.
    /// </summary>
    [Serializable]
    public class OnTriggerMergeDesc : OnTriggerWindowDesc
    {
        public OnTriggerMergeDesc(
            string windowName,
            string optionalAsName,
            OnTriggerMergeActionInsert optionalInsertNoMatch,
            IList<OnTriggerMergeMatched> items)
            : base(windowName, optionalAsName, OnTriggerType.ON_MERGE, false)
        {
            OptionalInsertNoMatch = optionalInsertNoMatch;
            Items = items;
        }

        public IList<OnTriggerMergeMatched> Items { get; }

        public OnTriggerMergeActionInsert OptionalInsertNoMatch { get; }
    }
} // end of namespace