///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.join.lookup;

namespace com.espertech.esper.common.@internal.epl.namedwindow.core
{
    /// <summary>
    ///     An instance of this class is associated with a specific named window. The processor
    ///     provides the views to create-window, on-delete statements and statements selecting from a named window.
    /// </summary>
    public class NamedWindowInstance
    {
        public NamedWindowInstance(
            NamedWindow processor,
            AgentInstanceContext agentInstanceContext)
        {
            RootViewInstance = new NamedWindowRootViewInstance(
                processor.RootView,
                agentInstanceContext,
                processor.EventTableIndexMetadata);
            TailViewInstance = new NamedWindowTailViewInstance(
                RootViewInstance,
                processor.TailView,
                processor,
                agentInstanceContext);
            RootViewInstance.DataWindowContents = TailViewInstance; // for iteration used for delete without index
        }

        public NamedWindowRootViewInstance RootViewInstance { get; }

        public NamedWindowTailViewInstance TailViewInstance { get; }

        public IndexMultiKey[] IndexDescriptors => RootViewInstance.Indexes;

        public long CountDataWindow => TailViewInstance.NumberOfEvents;

        public void Destroy()
        {
            TailViewInstance.Destroy();
            RootViewInstance.Destroy();
        }

        public void RemoveIndex(IndexMultiKey index)
        {
            RootViewInstance.IndexRepository.RemoveIndex(index);
        }

        public void RemoveExplicitIndex(string indexName)
        {
            RootViewInstance.IndexRepository.RemoveExplicitIndex(indexName);
        }
    }
} // end of namespace