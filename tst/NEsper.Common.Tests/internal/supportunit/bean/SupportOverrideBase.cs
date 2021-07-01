///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.support;

namespace com.espertech.esper.common.@internal.supportunit.bean
{
    [Serializable]
    public class SupportOverrideBase : SupportMarkerInterface
    {
        private readonly string val;

        public SupportOverrideBase(string val)
        {
            this.val = val;
        }

        public virtual string Val
        {
            get => val;
        }
    }
} // end of namespace
