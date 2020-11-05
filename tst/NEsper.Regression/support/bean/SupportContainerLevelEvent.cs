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
    public class SupportContainerLevelEvent
    {
        private readonly ISet<SupportContainerLevel1Event> level1s;

        public SupportContainerLevelEvent(ISet<SupportContainerLevel1Event> level1s)
        {
            this.level1s = level1s;
        }

        public ISet<SupportContainerLevel1Event> GetLevel1s()
        {
            return level1s;
        }
    }
} // end of namespace