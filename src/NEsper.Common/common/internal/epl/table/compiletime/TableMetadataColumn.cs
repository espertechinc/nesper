///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.table.compiletime
{
    public abstract class TableMetadataColumn
    {
        protected TableMetadataColumn()
        {
        }

        protected TableMetadataColumn(
            string columnName,
            bool key)
        {
            ColumnName = columnName;
            IsKey = key;
        }

        public bool IsKey { get; set; }

        public string ColumnName { get; set; }

        protected abstract CodegenExpression Make(
            CodegenMethodScope parent,
            ModuleTableInitializeSymbol symbols,
            CodegenClassScope classScope);

        internal void MakeSettersInline(
            CodegenExpressionRef col,
            CodegenBlock block)
        {
            block.SetProperty(col, "IsKey", Constant(IsKey))
                .SetProperty(col, "ColumnName", Constant(ColumnName));
        }

        public static CodegenExpression MakeColumns(
            IDictionary<string, TableMetadataColumn> columns,
            CodegenMethodScope parent,
            ModuleTableInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(
                typeof(IDictionary<string, TableMetadataColumn>),
                typeof(TableMetadataColumn),
                classScope);
            method.Block.DeclareVar<IDictionary<string, TableMetadataColumn>>(
                "cols",
                NewInstance(typeof(Dictionary<string, TableMetadataColumn>)));
            foreach (var entry in columns) {
                method.Block.ExprDotMethod(
                    Ref("cols"),
                    "Put",
                    Constant(entry.Key),
                    entry.Value.Make(method, symbols, classScope));
            }

            method.Block.MethodReturn(Ref("cols"));
            return LocalMethod(method);
        }
    }
} // end of namespace