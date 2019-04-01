///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.hook;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.supportregression.virtualdw
{
    public class SupportVirtualDWInvalid : VirtualDataWindow
    {
        public VirtualDataWindowLookup GetLookup(VirtualDataWindowLookupContext desc)
        {
            return null;
        }
    
        public void Update(EventBean[] newData, EventBean[] oldData)
        {
        }
    
        public void Dispose()
        {
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<EventBean> GetEnumerator()
        {
            return EnumerationHelper.Empty<EventBean>();
        }
    
        public void HandleEvent(VirtualDataWindowEvent theEvent)
        {
        }
    }
}
