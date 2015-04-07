///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.filter;
using com.espertech.esper.schedule;

namespace com.espertech.esper.core.service
{
    /// <summary>Context for all services that provide the isolated runtime. </summary>
    public class EPIsolationUnitServices
    {
        /// <summary>Ctor. </summary>
        /// <param name="name">the isolation unit name</param>
        /// <param name="unitId">id of the isolation unit</param>
        /// <param name="filterService">isolated filter service</param>
        /// <param name="schedulingService">isolated scheduling service</param>
        public EPIsolationUnitServices(String name,
                                       int unitId,
                                       FilterServiceSPI filterService,
                                       SchedulingServiceSPI schedulingService)
        {
            Name = name;
            UnitId = unitId;
            FilterService = filterService;
            SchedulingService = schedulingService;
        }

        /// <summary>Returns the name of the isolated service. </summary>
        /// <value>name of the isolated service</value>
        public string Name { get; private set; }

        /// <summary>Returns the id assigned to that isolated service. </summary>
        /// <value>isolated service id</value>
        public int UnitId { get; private set; }

        /// <summary>Returns the isolated filter service. </summary>
        /// <value>filter service</value>
        public FilterServiceSPI FilterService { get; private set; }

        /// <summary>Returns the isolated scheduling service. </summary>
        /// <value>scheduling service</value>
        public SchedulingServiceSPI SchedulingService { get; private set; }
    }
}