///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.spec
{
    /// <summary>Specification for the merge statement Update-part. </summary>
    [Serializable]
    public class OnTriggerMergeActionUpdate : OnTriggerMergeAction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OnTriggerMergeActionUpdate"/> class.
        /// </summary>
        /// <param name="optionalMatchCond">The optional match cond.</param>
        /// <param name="assignments">The assignments.</param>
        public OnTriggerMergeActionUpdate(ExprNode optionalMatchCond, IList<OnTriggerSetAssignment> assignments)
            : base(optionalMatchCond)
        {
            Assignments = assignments;
        }

        public IList<OnTriggerSetAssignment> Assignments { get; private set; }
    }
    
}
