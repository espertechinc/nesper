///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.common.@internal.epl.table.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.fafquery.processor
{
    public class FireAndForgetProcessorTableForge : FireAndForgetProcessorForge
    {
        public FireAndForgetProcessorTableForge(TableMetaData table)
        {
            Table = table;
        }

        public TableMetaData Table { get; }

        public string NamedWindowOrTableName => Table.TableName;

        public string ContextName => Table.OptionalContextName;

        public EventType EventTypeRspInputEvents => Table.InternalEventType;

        public EventType EventTypePublic => Table.PublicEventType;

        public string[][] UniqueIndexes => Table.IndexMetadata.UniqueIndexProps;

        public CodegenExpression Make(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(FireAndForgetProcessorTable), GetType(), classScope);
            var nw = Ref("tbl");
            method.Block
                .DeclareVar<FireAndForgetProcessorTable>(
                    nw.Ref,
                    NewInstance(typeof(FireAndForgetProcessorTable)))
                .SetProperty(
                    nw,
                    "Table",
                    TableDeployTimeResolver.MakeResolveTable(Table, symbols.GetAddInitSvc(method)))
                .MethodReturn(nw);
            return LocalMethod(method);
        }
    }
} // end of namespace