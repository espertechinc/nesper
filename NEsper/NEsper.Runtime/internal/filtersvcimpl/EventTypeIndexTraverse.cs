///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.filtersvc;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.runtime.@internal.filtersvcimpl
{

    public delegate void EventTypeIndexTraverse(
        ArrayDeque<FilterItem> stack,
        FilterHandle filterHandle);

#if FALSE
    public interface EventTypeIndexTraverse
    {
        void Add(ArrayDeque<FilterItem> stack, FilterHandle filterHandle);
    }
#endif
} // end of namespace