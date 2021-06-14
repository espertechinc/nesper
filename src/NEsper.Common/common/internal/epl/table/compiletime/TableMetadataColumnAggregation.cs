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
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.rettype;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.table.compiletime
{
    public class TableMetadataColumnAggregation : TableMetadataColumn
    {
        public TableMetadataColumnAggregation()
        {
        }

        public TableMetadataColumnAggregation(
            string columnName,
            bool key,
            int column,
            AggregationPortableValidation aggregationPortableValidation,
            string aggregationExpression,
            bool methodAgg,
            EPType optionalEnumerationType)
            : base(columnName, key)
        {
            Column = column;
            AggregationPortableValidation = aggregationPortableValidation;
            AggregationExpression = aggregationExpression;
            IsMethodAgg = methodAgg;
            OptionalEnumerationType = optionalEnumerationType;
        }

        public int Column { get; set; }

        public AggregationPortableValidation AggregationPortableValidation { get; set; }

        public string AggregationExpression { get; set; }

        public bool IsMethodAgg { get; set; }

        public EPType OptionalEnumerationType { get; set; }

        protected override CodegenExpression Make(
            CodegenMethodScope parent,
            ModuleTableInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(TableMetadataColumnAggregation), GetType(), classScope);
            method.Block.DeclareVar<TableMetadataColumnAggregation>(
                "col",
                NewInstance(typeof(TableMetadataColumnAggregation)));

            base.MakeSettersInline(Ref("col"), method.Block);
            method.Block
                .SetProperty(Ref("col"), "Column", Constant(Column))
                .SetProperty(
                    Ref("col"),
                    "AggregationPortableValidation",
                    AggregationPortableValidation.Make(method, symbols, classScope))
                .SetProperty(Ref("col"), "AggregationExpression", Constant(AggregationExpression))
                .SetProperty(Ref("col"), "IsMethodAgg", Constant(IsMethodAgg))
                .SetProperty(
                    Ref("col"),
                    "OptionalEnumerationType",
                    OptionalEnumerationType == null
                        ? ConstantNull()
                        : OptionalEnumerationType.Codegen(method, classScope, symbols.GetAddInitSvc(method)))
                .MethodReturn(Ref("col"));
            return LocalMethod(method);
        }
    }
} // end of namespace