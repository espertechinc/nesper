///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.named;

namespace com.espertech.esper.core.start
{
    public class FireAndForgetProcessorNamedWindow : FireAndForgetProcessor
    {
        private readonly NamedWindowProcessor _namedWindowProcessor;
    
        internal FireAndForgetProcessorNamedWindow(NamedWindowProcessor namedWindowProcessor) 
        {
            this._namedWindowProcessor = namedWindowProcessor;
        }

        public NamedWindowProcessor NamedWindowProcessor
        {
            get { return _namedWindowProcessor; }
        }

        public override EventType EventTypeResultSetProcessor
        {
            get { return _namedWindowProcessor.NamedWindowType; }
        }

        public override EventType EventTypePublic
        {
            get { return _namedWindowProcessor.NamedWindowType; }
        }

        public override string ContextName
        {
            get { return _namedWindowProcessor.ContextName; }
        }

        public override FireAndForgetInstance GetProcessorInstance(AgentInstanceContext agentInstanceContext)
        {
            NamedWindowProcessorInstance processorInstance = _namedWindowProcessor.GetProcessorInstance(agentInstanceContext);
            if (processorInstance != null) {
                return new FireAndForgetInstanceNamedWindow(processorInstance);
            }
            return null;
        }
    
        public override FireAndForgetInstance GetProcessorInstanceContextById(int agentInstanceId)
        {
            NamedWindowProcessorInstance processorInstance = _namedWindowProcessor.GetProcessorInstance(agentInstanceId);
            if (processorInstance != null) {
                return new FireAndForgetInstanceNamedWindow(processorInstance);
            }
            return null;
        }
    
        public override FireAndForgetInstance GetProcessorInstanceNoContext()
        {
            NamedWindowProcessorInstance processorInstance = _namedWindowProcessor.ProcessorInstanceNoContext;
            if (processorInstance == null) {
                return null;
            }
            return new FireAndForgetInstanceNamedWindow(processorInstance);
        }
    
        public override ICollection<int> GetProcessorInstancesAll()
        {
            return _namedWindowProcessor.GetProcessorInstancesAll();
        }

        public override string NamedWindowOrTableName
        {
            get { return _namedWindowProcessor.NamedWindowName; }
        }

        public override bool IsVirtualDataWindow
        {
            get { return _namedWindowProcessor.IsVirtualDataWindow; }
        }

        public override string[][] GetUniqueIndexes(FireAndForgetInstance processorInstance)
        {
            if (processorInstance == null) {
                return new string[0][];
            }
            return _namedWindowProcessor.GetUniqueIndexes(((FireAndForgetInstanceNamedWindow) processorInstance).ProcessorInstance);
        }
    }
}
