///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.context.factory;
using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.lookup;

namespace com.espertech.esper.epl.named
{
	/// <summary>
	/// An instance of this class is associated with a specific named window. The processor
	/// provides the views to create-window, on-delete statements and statements selecting from a named window.
	/// </summary>
	public class NamedWindowProcessorInstance
	{
	    private readonly int? _agentInstanceId;
	    private readonly NamedWindowTailViewInstance _tailViewInstance;
	    private readonly NamedWindowRootViewInstance _rootViewInstance;

	    public NamedWindowProcessorInstance(
	        int? agentInstanceId, 
	        NamedWindowProcessor processor, 
	        AgentInstanceContext agentInstanceContext)
	    {
	        _agentInstanceId = agentInstanceId;
	        _rootViewInstance = new NamedWindowRootViewInstance(processor.RootView, agentInstanceContext, processor.EventTableIndexMetadataRepo);
	        _tailViewInstance = new NamedWindowTailViewInstance(_rootViewInstance, processor.TailView, processor, agentInstanceContext);
	        _rootViewInstance.DataWindowContents = _tailViewInstance;   // for iteration used for delete without index
	    }

	    public NamedWindowTailViewInstance TailViewInstance => _tailViewInstance;

	    public NamedWindowRootViewInstance RootViewInstance => _rootViewInstance;

	    /// <summary>
	    /// Returns the number of events held.
	    /// </summary>
	    /// <value>number of events</value>
	    public long CountDataWindow => _tailViewInstance.NumberOfEvents;

	    /// <summary>
	    /// Deletes a named window and removes any associated resources.
	    /// </summary>
	    public void Dispose()
	    {
	        _tailViewInstance.Destroy();
	        _rootViewInstance.Destroy();
	    }

	    public void Stop()
        {
	        _tailViewInstance.Stop();
	        _rootViewInstance.Stop();
	    }

	    public IndexMultiKey[] IndexDescriptors => _rootViewInstance.Indexes;

	    public int? AgentInstanceId => _agentInstanceId;

	    public StatementAgentInstancePostLoad PostLoad
	    {
	        get
	        {
	            return new ProxyStatementAgentInstancePostLoad()
	            {
	                ProcExecutePostLoad = () => _rootViewInstance.PostLoad(),
	                ProcAcceptIndexVisitor = (visitor) => _rootViewInstance.VisitIndexes(visitor),
	            };
	        }
	    }

	    public void RemoveIndex(IndexMultiKey index)
        {
	        _rootViewInstance.IndexRepository.RemoveIndex(index);
	    }

	    public void RemoveExplicitIndex(string indexName)
        {
	        _rootViewInstance.IndexRepository.RemoveExplicitIndex(indexName);
	    }
	}
} // end of namespace
