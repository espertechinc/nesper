///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.regressionlib.support.bean
{
    public class SupportOverrideBase : SupportMarkerInterface
    {
        public SupportOverrideBase(string val)
        {
            Val = val;
        }

        public virtual string Val { get; }
    }
} // end of namespace