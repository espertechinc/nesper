///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.threading;
using com.espertech.esper.core.context.factory;
using com.espertech.esper.core.context.mgr;
using com.espertech.esper.pattern;
using com.espertech.esper.view;

namespace com.espertech.esper.core.service.resource
{
    public class StatementResourceService
    {
        public StatementResourceHolder ResourcesUnpartitioned { get; private set; }

        public IDictionary<int, StatementResourceHolder> ResourcesPartitioned { get; private set; }

        public IDictionary<ContextStatePathKey, EvalRootState> ContextEndEndpoints { get; private set; }

        public IDictionary<ContextStatePathKey, EvalRootState> ContextStartEndpoints { get; private set; }

        public StatementResourceService(bool partitioned)
        {
            if (partitioned)
            {
                ResourcesPartitioned = new Dictionary<int, StatementResourceHolder>();
            }
        }

        public void StartContextPattern(EvalRootState patternStopCallback, bool startEndpoint, ContextStatePathKey path)
        {
            AddContextPattern(patternStopCallback, startEndpoint, path);
        }

        public void StopContextPattern(bool startEndpoint, ContextStatePathKey path)
        {
            RemoveContextPattern(startEndpoint, path);
        }

        public StatementResourceHolder GetPartitioned(int agentInstanceId)
        {
            return ResourcesPartitioned.Get(agentInstanceId);
        }

        public StatementResourceHolder Unpartitioned
        {
            get { return ResourcesUnpartitioned; }
            set { ResourcesUnpartitioned = value; }
        }

        public void SetPartitioned(int agentInstanceId, StatementResourceHolder statementResourceHolder)
        {
            ResourcesPartitioned.Put(agentInstanceId, statementResourceHolder);
        }

        public StatementResourceHolder DeallocatePartitioned(int agentInstanceId)
        {
            return ResourcesPartitioned.Delete(agentInstanceId);
        }

        public StatementResourceHolder DeallocateUnpartitioned()
        {
            StatementResourceHolder unpartitioned = ResourcesUnpartitioned;
            ResourcesUnpartitioned = null;
            return unpartitioned;
        }

        private void RemoveContextPattern(bool startEndpoint, ContextStatePathKey path)
        {
            if (startEndpoint)
            {
                if (ContextStartEndpoints != null)
                {
                    ContextStartEndpoints.Remove(path);
                }
            }
            else
            {
                if (ContextEndEndpoints != null)
                {
                    ContextEndEndpoints.Remove(path);
                }
            }
        }

        private void AddContextPattern(EvalRootState rootState, bool startEndpoint, ContextStatePathKey path)
        {
            if (startEndpoint)
            {
                if (ContextStartEndpoints == null)
                {
                    ContextStartEndpoints = new Dictionary<ContextStatePathKey, EvalRootState>();
                }
                ContextStartEndpoints.Put(path, rootState);
            }
            else
            {
                if (ContextEndEndpoints == null)
                {
                    ContextEndEndpoints = new Dictionary<ContextStatePathKey, EvalRootState>();
                }
                ContextEndEndpoints.Put(path, rootState);
            }
        }
    }
}