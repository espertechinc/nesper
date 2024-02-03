///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.annotation;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.fabric;

namespace com.espertech.esper.common.@internal.epl.agg.core
{
    public interface AggregationServiceFactoryForge
    {
        StateMgmtSetting StateMgmtSetting { set; }
        AppliesTo? AppliesTo();
        void AppendRowFabricType(FabricTypeCollector fabricTypeCollector);
        T Accept<T>(AggregationServiceFactoryForgeVisitor<T> visitor);
    }
} // end of namespace