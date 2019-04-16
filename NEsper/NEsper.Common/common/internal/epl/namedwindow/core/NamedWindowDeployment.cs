///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.namedwindow.path;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.namedwindow.core
{
    public class NamedWindowDeployment
    {
        private readonly IDictionary<string, NamedWindow> namedWindows = new Dictionary<string, NamedWindow>(4);

        public void Add(
            string windowName,
            NamedWindowMetaData metadata,
            EPStatementInitServices services)
        {
            NamedWindow existing = namedWindows.Get(windowName);
            if (existing != null) {
                throw new IllegalStateException("Named window processor already found for name '" + windowName + "'");
            }

            NamedWindow namedWindow = services.NamedWindowFactoryService.CreateNamedWindow(metadata, services);
            namedWindows.Put(windowName, namedWindow);
        }

        public NamedWindow GetProcessor(string namedWindowName)
        {
            return namedWindows.Get(namedWindowName);
        }

        public void Remove(string tableName)
        {
            namedWindows.Remove(tableName);
        }

        public bool IsEmpty()
        {
            return namedWindows.IsEmpty();
        }
    }
} // end of namespace