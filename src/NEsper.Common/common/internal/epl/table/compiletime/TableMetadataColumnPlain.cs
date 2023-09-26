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

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.table.compiletime
{
    public class TableMetadataColumnPlain : TableMetadataColumn
    {
        public TableMetadataColumnPlain()
        {
        }

        public TableMetadataColumnPlain(
            string columnName,
            bool key,
            int indexPlain)
            : base(columnName, key)
        {
            IndexPlain = indexPlain;
        }

        public int IndexPlain { get; set; }

        protected override CodegenExpression Make(
            CodegenMethodScope parent,
            ModuleTableInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(TableMetadataColumnPlain), GetType(), classScope);
            method.Block.DeclareVarNewInstance<TableMetadataColumnPlain>("col");
            MakeSettersInline(Ref("col"), method.Block);
            method.Block
                .SetProperty(Ref("col"), "IndexPlain", Constant(IndexPlain))
                .MethodReturn(Ref("col"));
            return LocalMethod(method);
        }
    }
} // end of namespace