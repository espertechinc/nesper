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

namespace com.espertech.esper.epl.core
{
    public class ResultSetProcessorRowPerGroupUnboundGroupRepImpl : ResultSetProcessorRowPerGroupUnboundGroupRep
    {
        private readonly IDictionary<object, EventBean> _groupReps = new LinkedHashMap<object, EventBean>();

        public void Put(object key, EventBean @event)
        {
            _groupReps.Put(key, @event);
        }

        public IEnumerable<EventBean> Values
        {
            get { return _groupReps.Values; }
        }

        public void Removed(object key)
        {
            _groupReps.Remove(key);
        }

        public void Destroy()
        {
            // no action required
        }
    }
} // end of namespace