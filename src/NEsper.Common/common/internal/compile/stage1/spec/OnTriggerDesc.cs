///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.@internal.compile.stage1.spec
{
    /// <summary>Specification for on-trigger statements.</summary>
    [Serializable]
    public abstract class OnTriggerDesc
    {
        /// <summary>Ctor.</summary>
        /// <param name="onTriggerType">the type of on-trigger</param>
        protected OnTriggerDesc(OnTriggerType onTriggerType)
        {
            OnTriggerType = onTriggerType;
        }

        /// <summary>Returns the type of the on-trigger statement.</summary>
        /// <returns>trigger type</returns>
        public OnTriggerType OnTriggerType { get; private set; }
    }
} // End of namespace