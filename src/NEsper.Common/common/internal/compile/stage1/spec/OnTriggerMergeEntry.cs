///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.compile.stage1.spec
{
    /// <summary>
    ///     Specification for the merge statement insert/update/delete-part.
    /// </summary>
    [Serializable]
    public abstract class OnTriggerMergeEntry
    {
        protected OnTriggerMergeEntry(ExprNode optionalMatchCond)
        {
            OptionalMatchCond = optionalMatchCond;
        }

        public ExprNode OptionalMatchCond { get; set; }
    }
} // end of namespace