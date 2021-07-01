///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace com.espertech.esper.regressionlib.support.bean
{
    [Serializable]
    public class SupportContainerLevel1Event
    {
        private readonly ISet<SupportContainerLevel2Event> level2s;

        public SupportContainerLevel1Event(ISet<SupportContainerLevel2Event> level2s)
        {
            this.level2s = level2s;
        }

        public ISet<SupportContainerLevel2Event> GetLevel2s()
        {
            return level2s;
        }
    }
} // end of namespace