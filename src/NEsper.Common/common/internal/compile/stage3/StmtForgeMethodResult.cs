///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.fabric;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.common.@internal.schedule;


namespace com.espertech.esper.common.@internal.compile.stage3
{
    public class StmtForgeMethodResult
    {
        private readonly IList<StmtClassForgeable> forgeables;
        private readonly IList<FilterSpecTracked> filtereds;
        private readonly IList<ScheduleHandleTracked> scheduleds;
        private readonly IList<NamedWindowConsumerStreamSpec> namedWindowConsumers;
        private readonly IList<FilterSpecParamExprNodeForge> filterBooleanExpressions;
        private readonly CodegenNamespaceScope namespaceScope;
        private readonly FabricCharge fabricCharge;

        public StmtForgeMethodResult(
            IList<StmtClassForgeable> forgeables,
            IList<FilterSpecTracked> filtereds,
            IList<ScheduleHandleTracked> scheduleds,
            IList<NamedWindowConsumerStreamSpec> namedWindowConsumers,
            IList<FilterSpecParamExprNodeForge> filterBooleanExpressions,
            CodegenNamespaceScope namespaceScope,
            FabricCharge fabricCharge)
        {
            this.forgeables = forgeables;
            this.filtereds = filtereds;
            this.scheduleds = scheduleds;
            this.namedWindowConsumers = namedWindowConsumers;
            this.filterBooleanExpressions = filterBooleanExpressions;
            this.namespaceScope = namespaceScope;
            this.fabricCharge = fabricCharge;
        }

        public IList<StmtClassForgeable> Forgeables => forgeables;

        public IList<ScheduleHandleTracked> Scheduleds => scheduleds;

        public IList<FilterSpecTracked> Filtereds => filtereds;

        public IList<NamedWindowConsumerStreamSpec> NamedWindowConsumers => namedWindowConsumers;

        public IList<FilterSpecParamExprNodeForge> FilterBooleanExpressions => filterBooleanExpressions;

        public CodegenNamespaceScope NamespaceScope => namespaceScope;

        public FabricCharge FabricCharge => fabricCharge;
    }
} // end of namespace