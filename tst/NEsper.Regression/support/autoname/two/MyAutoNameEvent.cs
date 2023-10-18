///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.regressionlib.support.autoname.two
{
    [Serializable]
    public class MyAutoNameEvent
    {
        public MyAutoNameEvent(string p0)
        {
            P0 = p0;
        }

        public string P0 { get; }
    }
} // end of namespace