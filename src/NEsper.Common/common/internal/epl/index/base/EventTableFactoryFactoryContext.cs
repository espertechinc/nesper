///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.settings;

namespace com.espertech.esper.common.@internal.epl.index.@base
{
    public interface EventTableFactoryFactoryContext
    {
        EventTableIndexService EventTableIndexService { get; }
        RuntimeSettingsService RuntimeSettingsService { get; }
        Attribute[] Annotations { get; }
    }
    
    public class ProxyEventTableFactoryFactoryContext : EventTableFactoryFactoryContext
    {
        public Func<EventTableIndexService> ProcEventTableIndexService { get; }
        public Func<RuntimeSettingsService> ProcRuntimeSettingsService { get; }
        public Func<Attribute[]> ProcAnnotations { get; }

        public EventTableIndexService EventTableIndexService => ProcEventTableIndexService.Invoke();
        public RuntimeSettingsService RuntimeSettingsService => ProcRuntimeSettingsService.Invoke();
        public Attribute[] Annotations => ProcAnnotations.Invoke();

        public ProxyEventTableFactoryFactoryContext()
        {
        }

        public ProxyEventTableFactoryFactoryContext(
            Func<EventTableIndexService> procEventTableIndexService,
            Func<RuntimeSettingsService> procRuntimeSettingsService,
            Func<Attribute[]> procAnnotations)
        {
            ProcEventTableIndexService = procEventTableIndexService;
            ProcRuntimeSettingsService = procRuntimeSettingsService;
            ProcAnnotations = procAnnotations;
        }
    }
} // end of namespace