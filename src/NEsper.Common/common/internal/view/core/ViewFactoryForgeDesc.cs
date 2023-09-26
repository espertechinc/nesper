///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.fabric;
using com.espertech.esper.common.@internal.schedule;

namespace com.espertech.esper.common.@internal.view.core
{
    public class ViewFactoryForgeDesc
    {
        public ViewFactoryForgeDesc(
            IList<ViewFactoryForge> forges,
            IList<StmtClassForgeableFactory> multikeyForges,
            IList<ScheduleHandleTracked> schedules,
            FabricCharge fabricCharge)
        {
            Forges = forges;
            MultikeyForges = multikeyForges;
            Schedules = schedules;
            FabricCharge = fabricCharge;
        }

        public IList<ViewFactoryForge> Forges { get; }

        public IList<StmtClassForgeableFactory> MultikeyForges { get; }

        public IList<ScheduleHandleTracked> Schedules { get; }

        public FabricCharge FabricCharge { get; }
    }
} // end of namespace