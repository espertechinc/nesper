///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.hook.vdw;
using com.espertech.esper.compat.collections;

namespace NEsper.Examples.VirtualDW
{
    public class SampleVirtualDataWindow : VirtualDataWindow
    {
        private readonly VirtualDataWindowContext _context;

        public SampleVirtualDataWindow(VirtualDataWindowContext context)
        {
            _context = context;
        }

        public VirtualDataWindowLookup GetLookup(VirtualDataWindowLookupContext desc)
        {
            // Place any code that interrogates the hash-index and btree-index fields here.

            // Return the index representation.
            return new SampleVirtualDataWindowLookup(_context);
        }

        public void Update(EventBean[] newData, EventBean[] oldData)
        {
            // This sample simply posts into the insert and remove stream what is received.
            _context.OutputStream.Update(newData, oldData);
        }

        public void HandleEvent(VirtualDataWindowEvent theEvent)
        {
        }

        public void Dispose()
        {
            // Called when the named window is stopped or destroyed, for each context partition.
            // This sample does not need to clean up resources.
        }

        public IEnumerator<EventBean> GetEnumerator()
        {
            return Collections.GetEmptyList<EventBean>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}