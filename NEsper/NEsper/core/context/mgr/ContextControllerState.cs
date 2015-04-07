///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.compat.collections;
using com.espertech.esper.core.context.util;

namespace com.espertech.esper.core.context.mgr
{
    public class ContextControllerState
    {
        private readonly OrderedDictionary<ContextStatePathKey, ContextStatePathValue> _states;
        private readonly bool _imported;
        private readonly ContextPartitionImportCallback _partitionImportCallback;
    
        public ContextControllerState(OrderedDictionary<ContextStatePathKey, ContextStatePathValue> states, bool imported, ContextPartitionImportCallback partitionImportCallback)
        {
            _states = states;
            _imported = imported;
            _partitionImportCallback = partitionImportCallback;
        }

        public OrderedDictionary<ContextStatePathKey, ContextStatePathValue> States
        {
            get { return _states; }
        }

        public bool IsImported
        {
            get { return _imported; }
        }

        public ContextPartitionImportCallback PartitionImportCallback
        {
            get { return _partitionImportCallback; }
        }
    }
}
