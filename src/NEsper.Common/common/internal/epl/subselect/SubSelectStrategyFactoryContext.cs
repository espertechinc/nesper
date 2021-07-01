///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.epl.index.@base;

namespace com.espertech.esper.common.@internal.epl.subselect
{
    public interface SubSelectStrategyFactoryContext
    {
        EventTableIndexService EventTableIndexService { get; }
        EventTableFactoryFactoryContext EventTableFactoryContext { get; }
    }

    public class ProxySubSelectStrategyFactoryContext : SubSelectStrategyFactoryContext
    {
        public Func<EventTableIndexService> ProcEventTableIndexService { get; }
        public Func<EventTableFactoryFactoryContext> ProcEventTableFactoryContext { get; }

        public EventTableIndexService EventTableIndexService =>
            ProcEventTableIndexService?.Invoke();

        public EventTableFactoryFactoryContext EventTableFactoryContext =>
            ProcEventTableFactoryContext.Invoke();

        public ProxySubSelectStrategyFactoryContext()
        {
        }

        public ProxySubSelectStrategyFactoryContext(Func<EventTableIndexService> procEventTableIndexService,
            Func<EventTableFactoryFactoryContext> procEventTableFactoryContext)
        {
            ProcEventTableIndexService = procEventTableIndexService;
            ProcEventTableFactoryContext = procEventTableFactoryContext;
        }
    }
}