///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;

namespace com.espertech.esper.common.@internal.@event.core
{
    public abstract class ProxyEventBeanManufacturer : EventBeanManufacturer
    {
        public delegate object MakeUnderlyingFunc(object[] properties);

        public MakeUnderlyingFunc ProcMakeUnderlying { get; set; }

        public abstract EventBean Make(object[] properties);

        public object MakeUnderlying(object[] properties)
        {
            return ProcMakeUnderlying(properties);
        }
    }
}