///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.context.util
{
    public class StatementDispatchTLEntry
    {
        private readonly ArrayDeque<UniformPair<EventBean[]>> results = new ArrayDeque<UniformPair<EventBean[]>>();
        private bool isDispatchWaiting;

        public ArrayDeque<UniformPair<EventBean[]>> Results => results;

        public bool IsDispatchWaiting {
            get => isDispatchWaiting;
            set => isDispatchWaiting = value;
        }
    }
} // end of namespace