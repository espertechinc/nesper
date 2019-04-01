///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.join.lookup;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.namedwindow.core
{
	/// <summary>
	/// An instance of this class is associated with a specific named window. The processor
	/// provides the views to create-window, on-delete statements and statements selecting from a named window.
	/// </summary>
	public class NamedWindowInstance {
	    private readonly NamedWindowRootViewInstance rootViewInstance;
	    private readonly NamedWindowTailViewInstance tailViewInstance;

	    public NamedWindowInstance(NamedWindow processor, AgentInstanceContext agentInstanceContext) {
	        rootViewInstance = new NamedWindowRootViewInstance(processor.RootView, agentInstanceContext, processor.EventTableIndexMetadata);
	        tailViewInstance = new NamedWindowTailViewInstance(rootViewInstance, processor.TailView, processor, agentInstanceContext);
	        rootViewInstance.DataWindowContents = tailViewInstance;   // for iteration used for delete without index
	    }

	    public NamedWindowRootViewInstance RootViewInstance {
	        get => rootViewInstance;
	    }

	    public NamedWindowTailViewInstance TailViewInstance {
	        get => tailViewInstance;
	    }

	    public void Destroy() {
	        tailViewInstance.Destroy();
	        rootViewInstance.Destroy();
	    }

	    public IndexMultiKey[] GetIndexDescriptors() {
	        return rootViewInstance.Indexes;
	    }

	    public void RemoveIndex(IndexMultiKey index) {
	        rootViewInstance.IndexRepository.RemoveIndex(index);
	    }

	    public long CountDataWindow {
	        get => tailViewInstance.NumberOfEvents;
	    }

	    public void RemoveExplicitIndex(string indexName) {
	        rootViewInstance.IndexRepository.RemoveExplicitIndex(indexName);
	    }
	}
} // end of namespace