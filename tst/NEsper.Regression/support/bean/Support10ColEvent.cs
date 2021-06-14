///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.regressionlib.support.bean
{
    [Serializable]
    public class Support10ColEvent
    {
        public Support10ColEvent(
            string groupKey,
            int value)
        {
            GroupKey = groupKey;
            C0 = value;
            C1 = value;
            C2 = value;
            C3 = value;
            C4 = value;
            C5 = value;
            C6 = value;
            C7 = value;
            C8 = value;
            C9 = value;
        }

        public string GroupKey { get; }

        public int C0 { get; }

        public int C1 { get; }

        public int C2 { get; }

        public int C3 { get; }

        public int C4 { get; }

        public int C5 { get; }

        public int C6 { get; }

        public int C7 { get; }

        public int C8 { get; }

        public int C9 { get; }
    }
} // end of namespace