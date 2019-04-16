///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

namespace com.espertech.esper.common.@internal.compile.stage1.spec
{
    /// <summary>Specification for the on-set statement.</summary>
    public class OnTriggerSetDesc : OnTriggerDesc
    {
        private readonly IList<OnTriggerSetAssignment> assignments;

        /// <summary> Ctor.</summary>
        /// <param name="assignments">is a list of assignments</param>
        public OnTriggerSetDesc(IList<OnTriggerSetAssignment> assignments)
            : base(OnTriggerType.ON_SET)
        {
            this.assignments = assignments;
        }

        /// <summary>Returns a list of all variables assignment by the on-set</summary>
        /// <returns>list of assignments</returns>
        public IList<OnTriggerSetAssignment> Assignments {
            get { return assignments; }
        }
    }
} // End of namespace