///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.filterspec
{
    public interface FilterSharedLookupableRegistery
    {
        void RegisterLookupable(
            EventType eventType,
            ExprFilterSpecLookupable lookupable);
    }

    public class ProxyFilterSharedLookupableRegistery : FilterSharedLookupableRegistery
    {
        public Action<EventType, ExprFilterSpecLookupable> ProcRegisterLookupable { get; set; }

        public void RegisterLookupable(
            EventType eventType,
            ExprFilterSpecLookupable lookupable)
        {
            ProcRegisterLookupable?.Invoke(eventType, lookupable);
        }
    }
} // end of namespace