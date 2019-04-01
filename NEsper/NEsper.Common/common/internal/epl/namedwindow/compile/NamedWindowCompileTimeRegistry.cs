///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.namedwindow.path;
using com.espertech.esper.common.@internal.epl.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.namedwindow.compile
{
	public class NamedWindowCompileTimeRegistry : CompileTimeRegistry {
	    private readonly IDictionary<string, NamedWindowMetaData> namedWindows = new Dictionary<string,  NamedWindowMetaData>();

	    public void NewNamedWindow(NamedWindowMetaData detail) {
	        EventType eventType = detail.EventType;
	        if (!eventType.Metadata.AccessModifier.IsModuleProvidedAccessModifier) {
	            throw new IllegalStateException("Invalid visibility for named window");
	        }
	        string namedWindowName = detail.EventType.Name;
	        NamedWindowMetaData existing = namedWindows.Get(namedWindowName);
	        if (existing != null) {
	            throw new IllegalStateException("Duplicate named window definition encountered");
	        }
	        namedWindows.Put(namedWindowName, detail);
	    }

	    public bool IsNamedWindow(string namedWindowName) {
	        return namedWindows.ContainsKey(namedWindowName);
	    }

	    public EventType GetEventType(string namedWindowName) {
	        return namedWindows.Get(namedWindowName).EventType;
	    }

	    public IDictionary<string, NamedWindowMetaData> GetNamedWindows() {
	        return namedWindows;
	    }
	}
} // end of namespace