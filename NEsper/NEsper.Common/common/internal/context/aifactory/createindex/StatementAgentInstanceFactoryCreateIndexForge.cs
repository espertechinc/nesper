///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.join.lookup;
using com.espertech.esper.common.@internal.epl.join.queryplan;
using com.espertech.esper.common.@internal.epl.namedwindow.core;
using com.espertech.esper.common.@internal.epl.namedwindow.path;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.common.@internal.epl.table.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.context.aifactory.createindex
{
    public class StatementAgentInstanceFactoryCreateIndexForge
    {
        private readonly EventType eventType;
        private readonly string indexName;
        private readonly string indexModuleName;
        private readonly QueryPlanIndexItemForge explicitIndexDesc;
        private readonly IndexMultiKey imk;
        private readonly NamedWindowMetaData namedWindow;
        private readonly TableMetaData table;

        public StatementAgentInstanceFactoryCreateIndexForge(
            EventType eventType,
            string indexName,
            string indexModuleName,
            QueryPlanIndexItemForge explicitIndexDesc,
            IndexMultiKey imk,
            NamedWindowMetaData namedWindow,
            TableMetaData table)
        {
            this.eventType = eventType;
            this.indexName = indexName;
            this.indexModuleName = indexModuleName;
            this.explicitIndexDesc = explicitIndexDesc;
            this.imk = imk;
            this.namedWindow = namedWindow;
            this.table = table;
        }

        public CodegenMethod InitializeCodegen(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            CodegenMethod method = parent.MakeChild(typeof(StatementAgentInstanceFactoryCreateIndex), this.GetType(), classScope);
            CodegenExpressionRef saiff = @Ref("saiff");
            method.Block
                .DeclareVar(
                    typeof(StatementAgentInstanceFactoryCreateIndex), saiff.Ref, NewInstance(typeof(StatementAgentInstanceFactoryCreateIndex)))
                .SetProperty(saiff, "EventType", EventTypeUtility.ResolveTypeCodegen(eventType, symbols.GetAddInitSvc(method)))
                .SetProperty(saiff, "IndexName", Constant(indexName))
                .SetProperty(saiff, "IndexModuleName", Constant(indexModuleName))
                .SetProperty(saiff, "IndexMultiKey", imk.Make(method, classScope))
                .SetProperty(saiff, "ExplicitIndexDesc", explicitIndexDesc.Make(method, classScope));
            if (namedWindow != null) {
                method.Block.SetProperty(saiff, "NamedWindow", NamedWindowDeployTimeResolver.MakeResolveNamedWindow(namedWindow, symbols.GetAddInitSvc(method)));
            }
            else {
                method.Block.SetProperty(saiff, "Table", TableDeployTimeResolver.MakeResolveTable(table, symbols.GetAddInitSvc(method)));
            }

            method.Block.MethodReturn(saiff);
            return method;
        }
    }
} // end of namespace