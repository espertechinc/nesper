///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.schedule;

namespace com.espertech.esper.common.@internal.view.core
{
    public class ViewForgeVisitorSchedulesCollector : ViewForgeVisitor
    {
        private readonly IList<ScheduleHandleCallbackProvider> providers;

        public ViewForgeVisitorSchedulesCollector(IList<ScheduleHandleCallbackProvider> providers)
        {
            this.providers = providers;
        }

        public void Visit(ViewFactoryForge forge)
        {
            if (forge is ScheduleHandleCallbackProvider provider) {
                providers.Add(provider);
            }
        }
    }
} // end of namespace