///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.epl.namedwindow.path;
using com.espertech.esper.common.@internal.epl.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.namedwindow.compile
{
    public class NamedWindowCompileTimeRegistry : CompileTimeRegistry
    {
        public IDictionary<string, NamedWindowMetaData> NamedWindows { get; } =
            new Dictionary<string, NamedWindowMetaData>();

        public void NewNamedWindow(NamedWindowMetaData detail)
        {
            var eventType = detail.EventType;
            if (!eventType.Metadata.AccessModifier.IsModuleProvidedAccessModifier()) {
                throw new IllegalStateException("Invalid visibility for named window");
            }

            var namedWindowName = detail.EventType.Name;
            var existing = NamedWindows.Get(namedWindowName);
            if (existing != null) {
                throw new IllegalStateException("Duplicate named window definition encountered");
            }

            NamedWindows.Put(namedWindowName, detail);
        }

        public bool IsNamedWindow(string namedWindowName)
        {
            return NamedWindows.ContainsKey(namedWindowName);
        }

        public EventType GetEventType(string namedWindowName)
        {
            return NamedWindows.Get(namedWindowName).EventType;
        }
    }
} // end of namespace