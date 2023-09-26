///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

namespace com.espertech.esper.common.@internal.context.cpidsvc
{
    public interface ContextPartitionIdService
    {
        int AllocateId(object[] partitionKeys);

        ICollection<int> Ids { get; }

        object[] GetPartitionKeys(int id);

        void RemoveId(int id);

        void Destroy();

        void Clear();
        
        void ClearCaches();

        long Count { get; }
    }
} // end of namespace