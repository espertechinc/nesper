///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.common.@internal.schedule;

namespace com.espertech.esper.common.@internal.compile.stage3
{
    public class StmtForgeMethodResult
    {
        public StmtForgeMethodResult(
            IList<StmtClassForgable> forgables,
            IList<FilterSpecCompiled> filtereds,
            IList<ScheduleHandleCallbackProvider> scheduleds,
            IList<NamedWindowConsumerStreamSpec> namedWindowConsumers,
            IList<FilterSpecParamExprNodeForge> filterBooleanExpressions)
        {
            Forgables = forgables;
            Filtereds = filtereds;
            Scheduleds = scheduleds;
            NamedWindowConsumers = namedWindowConsumers;
            FilterBooleanExpressions = filterBooleanExpressions;
        }

        public IList<StmtClassForgable> Forgables { get; }

        public IList<ScheduleHandleCallbackProvider> Scheduleds { get; }

        public IList<FilterSpecCompiled> Filtereds { get; }

        public IList<NamedWindowConsumerStreamSpec> NamedWindowConsumers { get; }

        public IList<FilterSpecParamExprNodeForge> FilterBooleanExpressions { get; }
    }
} // end of namespace