///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.compat.collections;
using com.espertech.esper.filter;

namespace com.espertech.esper.core.context.mgr
{
    public class AgentInstanceFilterProxyImpl : AgentInstanceFilterProxy
    {
        private readonly IdentityDictionary<FilterSpecCompiled, FilterValueSetParam[][]> _addendumMap;

        public AgentInstanceFilterProxyImpl(IdentityDictionary<FilterSpecCompiled, FilterValueSetParam[][]> addendums)
        {
            _addendumMap = addendums;
        }

        public FilterValueSetParam[][] GetAddendumFilters(FilterSpecCompiled filterSpec)
        {
            return _addendumMap.Get(filterSpec);
        }
    }
}
