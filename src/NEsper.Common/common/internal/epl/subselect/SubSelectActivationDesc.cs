///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.epl.expression.subquery;
using com.espertech.esper.common.@internal.fabric;
using com.espertech.esper.common.@internal.schedule;

namespace com.espertech.esper.common.@internal.epl.subselect
{
    public class SubSelectActivationDesc
    {
        public SubSelectActivationDesc(
            IDictionary<ExprSubselectNode, SubSelectActivationPlan> subselects,
            IList<StmtClassForgeableFactory> additionalForgeables,
            IList<ScheduleHandleTracked> schedules,
            FabricCharge fabricCharge)
        {
            Subselects = subselects;
            AdditionalForgeables = additionalForgeables;
            Schedules = schedules;
            FabricCharge = fabricCharge;
        }

        public IDictionary<ExprSubselectNode, SubSelectActivationPlan> Subselects { get; }

        public IList<StmtClassForgeableFactory> AdditionalForgeables { get; }

        public IList<ScheduleHandleTracked> Schedules { get; }

        public FabricCharge FabricCharge { get; }
    }
} // end of namespace