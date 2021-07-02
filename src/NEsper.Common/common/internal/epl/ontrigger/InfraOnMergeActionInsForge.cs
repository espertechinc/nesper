///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.resultset.select.core;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.common.@internal.epl.table.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.ontrigger
{
    public class InfraOnMergeActionInsForge : InfraOnMergeActionForge
    {
        private readonly SelectExprProcessorForge insertHelper;
        private readonly TableMetaData insertIntoTable;
        private readonly bool audit;
        private readonly bool route;

        public InfraOnMergeActionInsForge(
            ExprNode optionalFilter,
            SelectExprProcessorForge insertHelper,
            TableMetaData insertIntoTable,
            bool audit,
            bool route)
            : base(optionalFilter)

        {
            this.insertHelper = insertHelper;
            this.insertIntoTable = insertIntoTable;
            this.audit = audit;
            this.route = route;
        }

        public TableMetaData InsertIntoTable {
            get => insertIntoTable;
        }

        public override CodegenExpression Make(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(InfraOnMergeActionIns), GetType(), classScope);
            var anonymousSelect = SelectExprProcessorUtil.MakeAnonymous(
                insertHelper,
                method,
                symbols.GetAddInitSvc(method),
                classScope);
            method.Block.MethodReturn(
                NewInstance<InfraOnMergeActionIns>(
                    MakeFilter(method, classScope),
                    anonymousSelect,
                    insertIntoTable == null
                        ? ConstantNull()
                        : TableDeployTimeResolver.MakeResolveTable(insertIntoTable, symbols.GetAddInitSvc(method)),
                    Constant(audit),
                    Constant(route)));
            return LocalMethod(method);
        }
    }
} // end of namespace