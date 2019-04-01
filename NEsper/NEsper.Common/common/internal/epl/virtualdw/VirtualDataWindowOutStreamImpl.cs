///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.hook.vdw;
using com.espertech.esper.common.@internal.view.core;

namespace com.espertech.esper.common.@internal.epl.virtualdw
{
    public class VirtualDataWindowOutStreamImpl : VirtualDataWindowOutStream
    {
        private ViewSupport view;

        public ViewSupport View {
            set => view = value;
        }

        public void Update(EventBean[] newData, EventBean[] oldData)
        {
            view.Child.Update(newData, oldData);
        }
    }
} // end of namespace