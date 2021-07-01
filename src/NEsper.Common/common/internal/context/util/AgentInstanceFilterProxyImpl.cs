///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.function;

namespace com.espertech.esper.common.@internal.context.util
{
    public class AgentInstanceFilterProxyImpl : AgentInstanceFilterProxy
    {
        private Supplier<IDictionary<FilterSpecActivatable, FilterValueSetParam[][]>> _generator;
        private IDictionary<FilterSpecActivatable, FilterValueSetParam[][]> _addendumMap;

        public AgentInstanceFilterProxyImpl(Supplier<IDictionary<FilterSpecActivatable, FilterValueSetParam[][]>> generator)
        {
            this._generator = generator;
        }

        public FilterValueSetParam[][] GetAddendumFilters(
            FilterSpecActivatable filterSpec,
            AgentInstanceContext agentInstanceContext)
        {
            if (_addendumMap == null) {
                _addendumMap = _generator.Invoke();
                if (_addendumMap.IsEmpty()) {
                    _addendumMap = EmptyDictionary<FilterSpecActivatable, FilterValueSetParam[][]>.Instance;
                }

                _generator = null;
            }

            return _addendumMap.Get(filterSpec);
        }
    }
} // end of namespace