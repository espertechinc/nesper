///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.core.thread
{
    /// <summary>Ctor </summary>
    public class ThreadingOption
    {
        static ThreadingOption()
        {
            IsThreadingEnabled = false;
        }

        /// <summary>Returns true when threading is enabled </summary>
        /// <value>indicator</value>
        public static bool IsThreadingEnabled { get; set; }
    }
}
