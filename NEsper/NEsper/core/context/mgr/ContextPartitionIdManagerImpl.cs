///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

namespace com.espertech.esper.core.context.mgr
{
    public class ContextPartitionIdManagerImpl : ContextPartitionIdManager
    {
        private int _lastAssignedId = -1;

        public ContextPartitionIdManagerImpl()
        {
            Ids = new HashSet<int>();
        }

        #region ContextPartitionIdManager Members

        public void Clear()
        {
            Ids.Clear();
        }

        public void AddExisting(int contextPartitionId)
        {
            Ids.Add(contextPartitionId);
        }

        public int AllocateId()
        {
            while (true)
            {
                if (_lastAssignedId < int.MaxValue)
                {
                    _lastAssignedId++;
                }
                else
                {
                    _lastAssignedId = 0;
                }

                if (!Ids.Contains(_lastAssignedId))
                {
                    Ids.Add(_lastAssignedId);
                    return _lastAssignedId;
                }
            }
        }

        public void RemoveId(int contextPartitionId)
        {
            Ids.Remove(contextPartitionId);
        }

        #endregion

        public ICollection<int> Ids { get; private set; }
    }
}