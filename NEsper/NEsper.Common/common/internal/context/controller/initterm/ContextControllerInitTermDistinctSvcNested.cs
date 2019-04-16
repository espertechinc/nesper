///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.context.controller.initterm
{
    public class ContextControllerInitTermDistinctSvcNested : ContextControllerInitTermDistinctSvc
    {
        private readonly IDictionary<IntSeqKey, ISet<object>> distinctContexts = new Dictionary<IntSeqKey, ISet<object>>();

        public ContextControllerInitTermDistinctSvcNested()
        {
        }

        public bool AddUnlessExists(
            IntSeqKey controllerPath,
            object key)
        {
            ISet<object> keys = distinctContexts.Get(controllerPath);
            if (keys == null) {
                keys = new HashSet<object>();
                distinctContexts.Put(controllerPath, keys);
            }

            return keys.Add(key);
        }

        public void Remove(
            IntSeqKey controllerPath,
            object key)
        {
            ISet<object> keys = distinctContexts.Get(controllerPath);
            if (keys == null) {
                return;
            }

            keys.Remove(key);
        }

        public void Clear(IntSeqKey path)
        {
            distinctContexts.Remove(path);
        }

        public void Destroy()
        {
            distinctContexts.Clear();
        }
    }
} // end of namespace