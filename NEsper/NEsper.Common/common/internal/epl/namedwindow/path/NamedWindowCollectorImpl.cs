///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.namedwindow.path
{
    public class NamedWindowCollectorImpl : NamedWindowCollector
    {
        private readonly IDictionary<string, NamedWindowMetaData> moduleNamedWindows;

        public NamedWindowCollectorImpl(IDictionary<string, NamedWindowMetaData> moduleNamedWindows)
        {
            this.moduleNamedWindows = moduleNamedWindows;
        }

        public void RegisterNamedWindow(
            string namedWindowName,
            NamedWindowMetaData namedWindow)
        {
            moduleNamedWindows.Put(namedWindowName, namedWindow);
        }
    }
} // end of namespace