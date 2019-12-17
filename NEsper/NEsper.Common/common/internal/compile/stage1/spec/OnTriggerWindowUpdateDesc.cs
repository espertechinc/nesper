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
    /// Specification for the on-select and on-delete (no split-stream) statement.
    /// </summary>
    [Serializable]
    public class OnTriggerWindowUpdateDesc : OnTriggerWindowDesc
    {
        /// <summary>Ctor. </summary>
        /// <param name="windowName">the window name</param>
        /// <param name="optionalAsName">the optional name</param>
        /// <param name="assignments">set-assignments</param>
        public OnTriggerWindowUpdateDesc(
            String windowName,
            String optionalAsName,
            IList<OnTriggerSetAssignment> assignments)
            : base(windowName, optionalAsName, spec.OnTriggerType.ON_UPDATE, false)
        {
            Assignments = assignments;
        }

        /// <summary>Returns assignments. </summary>
        /// <returns>assignments</returns>
        public IList<OnTriggerSetAssignment> Assignments { get; private set; }
    }
}