///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.collection;

namespace com.espertech.esper.common.@internal.context.controller.initterm
{
    public class ContextControllerInitTermDistinctSvcNonNested : ContextControllerInitTermDistinctSvc
    {
        private readonly ISet<object> _distinctContexts = new HashSet<object>();

        public ContextControllerInitTermDistinctSvcNonNested()
        {
        }

        public bool AddUnlessExists(
            IntSeqKey controllerPath,
            object key)
        {
            return _distinctContexts.Add(key);
        }

        public void Remove(
            IntSeqKey controllerPath,
            object key)
        {
            _distinctContexts.Remove(key);
        }

        public void Clear(IntSeqKey path)
        {
            _distinctContexts.Clear();
        }

        public void Destroy()
        {
            _distinctContexts.Clear();
        }
    }
} // end of namespace