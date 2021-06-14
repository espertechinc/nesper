///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.compile.stage1.spec
{
    /// <summary>Specification for the merge statement insert/Update/delete-part. </summary>
    [Serializable]
    public class OnTriggerMergeMatched
    {
        public OnTriggerMergeMatched(
            bool matchedUnmatched,
            ExprNode optionalMatchCond,
            IList<OnTriggerMergeAction> actions)
        {
            IsMatchedUnmatched = matchedUnmatched;
            OptionalMatchCond = optionalMatchCond;
            Actions = actions;
        }

        public ExprNode OptionalMatchCond { get; set; }

        public bool IsMatchedUnmatched { get; private set; }

        public IList<OnTriggerMergeAction> Actions { get; private set; }
    }
}