///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.epl.agg.service;

namespace com.espertech.esper.core.context.stmt
{
    public interface AIRegistryAggregation : AggregationService
    {
        int InstanceCount { get; }
        void AssignService(int serviceId, AggregationService aggregationService);
        void DeassignService(int serviceId);
    }
}