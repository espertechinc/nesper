///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.events
{
    public class EventAdapterServiceAnonymousTypeCache
    {
        private readonly int _size;
        private readonly LinkedList<EventTypeSPI> _recentTypes;

        public EventAdapterServiceAnonymousTypeCache(int size)
        {
            _size = size;
            _recentTypes = new LinkedList<EventTypeSPI>();
        }

        public EventType AddReturnExistingAnonymousType(EventType requiredType)
        {
            lock (this)
            {
                // only EventTypeSPI compliant implementations considered
                if (!(requiredType is EventTypeSPI))
                {
                    return requiredType;
                }

                // check recent types
                foreach (EventTypeSPI existing in _recentTypes)
                {
                    if (existing.GetType() == requiredType.GetType() &&
                        Collections.AreEqual(requiredType.PropertyNames, existing.PropertyNames) &&
                        Collections.AreEqual(requiredType.PropertyDescriptors, existing.PropertyDescriptors) &&
                        existing.EqualsCompareType(requiredType))
                    {
                        return existing;
                    }
                }

                // add, removing the oldest
                if (_recentTypes.Count == _size && !_recentTypes.IsEmpty())
                {
                    _recentTypes.RemoveFirst();
                }
                if (_recentTypes.Count < _size)
                {
                    _recentTypes.AddLast((EventTypeSPI) requiredType);
                }
                return requiredType;
            }
        }
    }
}
