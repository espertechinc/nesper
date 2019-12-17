///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.context.util
{
    public class AgentInstanceFilterProxyImpl : AgentInstanceFilterProxy
    {
        private readonly Func<AgentInstanceContext, IDictionary<FilterSpecActivatable, FilterValueSetParam[][]>>
            generator;

        private IDictionary<FilterSpecActivatable, FilterValueSetParam[][]> addendumMap;

        public AgentInstanceFilterProxyImpl(
            Func<AgentInstanceContext, IDictionary<FilterSpecActivatable, FilterValueSetParam[][]>> generator)
        {
            this.generator = generator;
        }

        public FilterValueSetParam[][] GetAddendumFilters(
            FilterSpecActivatable filterSpec,
            AgentInstanceContext agentInstanceContext)
        {
            if (addendumMap == null) {
                addendumMap = generator.Invoke(agentInstanceContext);
            }

            return addendumMap.Get(filterSpec);
        }
    }
} // end of namespace