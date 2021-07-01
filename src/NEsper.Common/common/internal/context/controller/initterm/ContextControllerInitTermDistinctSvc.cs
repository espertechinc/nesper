///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.collection;

namespace com.espertech.esper.common.@internal.context.controller.initterm
{
    public interface ContextControllerInitTermDistinctSvc
    {
        bool AddUnlessExists(
            IntSeqKey controllerPath,
            object key);

        void Remove(
            IntSeqKey controllerPath,
            object key);

        void Clear(IntSeqKey path);

        void Destroy();
    }
} // end of namespace