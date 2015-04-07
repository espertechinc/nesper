///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.client.hook;
using com.espertech.esper.view;

namespace com.espertech.esper.epl.virtualdw
{
    public class VirtualDataWindowOutStreamImpl
        : VirtualDataWindowOutStream
    {
        private ViewSupport _view;
    
        public void SetView(ViewSupport view) {
            _view = view;
        }
    
        public void Update(EventBean[] newData, EventBean[] oldData) {
            _view.UpdateChildren(newData, oldData);
        }
    }
}
