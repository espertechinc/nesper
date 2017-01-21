///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.epl.core;

namespace com.espertech.esper.dataflow.ops.epl
{
    public class EPLSelectDeliveryCallback : SelectExprProcessorDeliveryCallback
    {
        public EventBean Selected(Object[] result)
        {
            Delivered = result;
            return null;
        }

        public void Reset()
        {
            Delivered = null;
        }

        public object[] Delivered { get; private set; }
    }
}